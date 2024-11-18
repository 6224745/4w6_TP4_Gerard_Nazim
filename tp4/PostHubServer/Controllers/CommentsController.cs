
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PostHubServer.Models.DTOs;
using PostHubServer.Models;
using PostHubServer.Services;
using System.Security.Claims;

namespace PostHubServer.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class CommentsController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly PostService _postService;
        private readonly CommentService _commentService;

        public CommentsController(UserManager<User> userManager, PostService postService, CommentService commentService)
        {
            _userManager = userManager;
            _postService = postService;
            _commentService = commentService;
        }

        // Créer un nouveau commentaire. (Ne permet pas de créer le commentaire principal d'un post, pour cela,
        // voir l'action PostPost dans PostsController)
        [HttpPost("{parentCommentId}")]
        [Authorize]
        public async Task<ActionResult<CommentDisplayDTO>> PostComment(int parentCommentId, CommentDTO commentDTO)
        {
            User? user = await _userManager.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            if (user == null) return Unauthorized();

            Comment? parentComment = await _commentService.GetComment(parentCommentId);
            if (parentComment == null || parentComment.User == null) return BadRequest();

            Comment? newComment = await _commentService.CreateComment(user, commentDTO.Text, parentComment);
            if (newComment == null) return StatusCode(StatusCodes.Status500InternalServerError);

            bool voteToggleSuccess = await _commentService.UpvoteComment(newComment.Id, user);
            if (!voteToggleSuccess) return StatusCode(StatusCodes.Status500InternalServerError);

            return Ok(new CommentDisplayDTO(newComment, false, user));
        }
        
        // Modifier le texte d'un commentaire
        [HttpPut("{commentId}")]
        [Authorize]
        public async Task<ActionResult<CommentDisplayDTO>> PutComment(int commentId, CommentDTO commentDTO)
        {
            User? user = await _userManager.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            Comment? comment = await _commentService.GetComment(commentId);
            if (comment == null) return NotFound();

            if (user == null || comment.User != user) return Unauthorized();

            Comment? editedComment = await _commentService.EditComment(comment, commentDTO.Text);
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

            Comment? comment = await _commentService.GetComment(commentId);
            if (comment == null) return NotFound();

            if (user == null || comment.User != user) return Unauthorized();

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
    }
}
