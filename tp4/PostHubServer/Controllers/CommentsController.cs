﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PostHubServer.Models.DTOs;
using PostHubServer.Models;
using PostHubServer.Services;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.Text.RegularExpressions;
using PostHubServer.Data;

namespace PostHubServer.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class CommentsController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly PostService _postService;
        private readonly CommentService _commentService;
        private readonly PostHubContext _hubContext;
        private readonly PictureService _pictureService;


        public CommentsController(UserManager<User> userManager, PostService postService, CommentService commentService, PictureService pictureService, PostHubContext postHubContext)
        {
            _userManager = userManager;
            _postService = postService;
            _commentService = commentService;
            _pictureService = pictureService;
            _hubContext = postHubContext;
        }

        // Créer un nouveau commentaire. (Ne permet pas de créer le commentaire principal d'un post, pour cela,
        // voir l'action PostPost dans PostsController)
        [HttpPost("{parentCommentId}")]
        [Authorize]
        public async Task<ActionResult<CommentDisplayDTO>> PostComment(int parentCommentId)
        {
            User? user = await _userManager.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            if (user == null) return Unauthorized();

            Comment? parentComment = await _commentService.GetComment(parentCommentId);
            if (parentComment == null || parentComment.User == null) return BadRequest();

            string comment = Request.Form["text"];
            if (comment == null) return BadRequest();

            IFormCollection formCollection = await Request.ReadFormAsync();
            int i = 0;
            IFormFile? file = formCollection.Files.GetFile("monImage" + i);
            List<Picture> pictures = new List<Picture>();

            while (file != null)
            {
                pictures.Add(await _pictureService.CreateCommentPicture(file));
                i++;
                file = formCollection.Files.GetFile("monImage" + i);

            }

            Comment? newComment = await _commentService.CreateComment(user, comment, parentComment, pictures);
            if (newComment == null) return StatusCode(StatusCodes.Status500InternalServerError);

            bool voteToggleSuccess = await _commentService.UpvoteComment(newComment.Id, user);
            if (!voteToggleSuccess) return StatusCode(StatusCodes.Status500InternalServerError);

            return Ok(new CommentDisplayDTO(newComment, false, user));
        }

        // Modifier le texte d'un commentaire
        [HttpPut("{commentId}")]
        [Authorize]
        public async Task<ActionResult<CommentDisplayDTO>> PutComment(int commentId)
        {
            IFormCollection formCollection = await Request.ReadFormAsync();
            int i = 0;
            IFormFile? file = formCollection.Files.GetFile("monImage" + i);
            string Comment = Request.Form["Comment"];
            if (Comment == null) return BadRequest();

            User? user = await _userManager.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            Comment? comment = await _commentService.GetComment(commentId);
            List<Picture> pictures = new List<Picture>();
            while (file != null)
            {
                pictures.Add(await _pictureService.CreateCommentPicture(file));

                i++;
                file = formCollection.Files.GetFile("monImage" + i);

            }
            if (comment == null) return NotFound();

            if (user == null || comment.User != user) return Unauthorized();

            Comment? editedComment = await _commentService.EditComment(comment, Comment, pictures);
            if (editedComment == null) return StatusCode(StatusCodes.Status500InternalServerError);

            return Ok(new CommentDisplayDTO(editedComment, true, user));
        }

        // Upvoter (ou annuler l'upvote) un commentaire
        [HttpPut("{commentId}")]
        [Authorize]
        public async Task<ActionResult> UpvoteComment(int commentId)
        {
            User? user = await _userManager.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            if (user == null) return BadRequest();

            bool voteToggleSuccess = await _commentService.UpvoteComment(commentId, user);
            if (!voteToggleSuccess) return StatusCode(StatusCodes.Status500InternalServerError);

            return Ok(new { Message = "Vote complété." });
        }

        // Downvoter (ou annuler le downvote) un commentaire
        [HttpPut("{commentId}")]
        [Authorize]
        public async Task<ActionResult> DownvoteComment(int commentId)
        {
            User? user = await _userManager.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            if (user == null) return BadRequest();

            bool voteToggleSuccess = await _commentService.DownvoteComment(commentId, user);
            if (!voteToggleSuccess) return StatusCode(StatusCodes.Status500InternalServerError);

            return Ok(new { Message = "Vote complété." });
        }

        // Supprimer un commentaire.
        // S'il possède des sous-commentaires -> Il sera soft-delete pour préserver les sous-commentaires.
        // S'il ne possède pas de sous-commentaires -> Il sera hard-delete.
        // Si c'est le commentaire principal d'un post et qu'il n'a pas de sous-commentaire -> Le post sera supprimé.
        [HttpDelete("{commentId}")]
        [Authorize]
        public async Task<ActionResult> DeleteComment(int commentId)
        {
            User? user = await _userManager.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            Picture picture = await _pictureService.GetCommentPicture(commentId);

            // Vérification du rôle d'admin
            bool isAdmin = await _userManager.IsInRoleAsync(user, "admin");
            Console.WriteLine($"Is user admin: {isAdmin}");


            Comment? comment = await _commentService.GetComment(commentId);
            if (comment == null) return NotFound();

            if (user == null || (comment.User != user && !isAdmin)) return Unauthorized();
            for (int i = 0; i <= comment.Pictures?.Count - 1; i++)
            {
                System.IO.File.Delete(Directory.GetCurrentDirectory() + "/images/thumbnail/" + comment.Pictures[i].FileName);
                System.IO.File.Delete(Directory.GetCurrentDirectory() + "/images/full/" + comment.Pictures[i].FileName);
                _hubContext.Pictures.Remove(comment.Pictures[i]);
            }
            // Cette boucle permet non-seulement de supprimer le commentaire lui-même, mais s'il possède
            // un commentaire parent qui a été soft-delete et qui n'a pas de sous-commentaires,
            // le supprime aussi. (Et ainsi de suite)
            do
            {
                comment.SubComments ??= new List<Comment>();

                Comment? parentComment = comment.ParentComment;

                // C'est un commentaire principal sans sous-commentaire :
                if (comment.MainCommentOf != null && comment.GetSubCommentTotal() == 0)
                {
                    Post? deletedPost = await _postService.DeletePost(comment.MainCommentOf);
                    if (deletedPost == null) return StatusCode(StatusCodes.Status500InternalServerError);
                }

                // Le commentaire n'a aucun sous-commentaire :
                if (comment.GetSubCommentTotal() == 0)
                {
                    Comment? deletedComment = await _commentService.HardDeleteComment(comment);
                    if (deletedComment == null) return StatusCode(StatusCodes.Status500InternalServerError);
                }
                // Le commentaire a des sous-commentaires :
                else
                {
                    Comment? deletedComment = await _commentService.SoftDeleteComment(comment);
                    if (deletedComment == null) return StatusCode(StatusCodes.Status500InternalServerError);
                    break;
                }

                comment = parentComment;

            } while (comment != null && comment.User == null && comment.GetSubCommentTotal() == 0);

            return Ok(new { Message = "Commentaire supprimé." });
        }

        [HttpGet("{size}/{id}")]
        public async Task<ActionResult> GetPicture(string size, int id)
        {
            Picture picture = await _pictureService.GetCommentPicture(id);
            if (picture == null || picture.FileName == null || picture.MimeType == null)
            {
                return NotFound(new { Message = "Cette image n'existe pas" });
            }
            if (!(Regex.Match(size, "full|thumbnail").Success))
            {
                return BadRequest(new { Message = "La taille demandée est inadéquate" });
            }
            string path = Directory.GetCurrentDirectory() + "/images/" + size + "/" + picture.FileName;
            byte[] bytes = System.IO.File.ReadAllBytes(path);
            return File(bytes, picture.MimeType);

        }

        [HttpGet("{id}")]
        public async Task<ActionResult<IEnumerable<int>>> GetCommentPictureIds(int id)
        {
            return await _pictureService.GetPictureIds(id);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<IEnumerable<int>>> GetCommentPicture(int id)
        {
            Picture? picture = await _pictureService.GetPictureId(id);

            byte[] bytes = System.IO.File.ReadAllBytes(Directory.GetCurrentDirectory() + "/images/thumbnail/" + picture.FileName);
            return File(bytes, picture.MimeType);
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<IEnumerable<int>>> GetFullCommentPicture(int id)
        {
            Picture? picture = await _pictureService.GetPictureId(id);

            byte[] bytes = System.IO.File.ReadAllBytes(Directory.GetCurrentDirectory() + "/images/full/" + picture.FileName);
            return File(bytes, picture.MimeType);
        }

        [HttpDelete("{id}")]
        public async Task DeletePicture(int id)
        {

            await _pictureService.DeletePictureById(id);


        }
    }
}
