﻿
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PostHubServer.Models;
using PostHubServer.Models.DTOs;
using PostHubServer.Services;
using System.Security.Claims;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace PostHubServer.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class PostsController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly HubService _hubService;
        private readonly PostService _postService;
        private readonly CommentService _commentService;
        private readonly PictureService _pictureService;

        public PostsController(UserManager<User> userManager, HubService hubService, PostService postService, CommentService commentService, PictureService pictureService)
        {
            _userManager = userManager;
            _hubService = hubService;
            _postService = postService;
            _commentService = commentService;
            _pictureService = pictureService;
        }

        // Créer un nouveau Post. Cela crée en fait un nouveau commentaire (le commentaire principal du post)
        // et le post lui-même.
        [HttpPost("{hubId}")]
        [Authorize]
        public async Task<ActionResult<PostDisplayDTO>> PostPost(int hubId, PostDTO postDTO)
        {
            //  IFormCollection formCollection = await Request.ReadFormAsync();
            //  IFormFile? file = formCollection.Files.GetFile("Image");
            //  if (file == null)
            //  {
            //  return BadRequest("Could not find the request image file");
            // }
            //  else
            //  {
            //  try
            //  {
            //  Image image = Image.Load(file.OpenReadStream());
            //
            //  Picture picture = new Picture();
            // picture.FileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            // picture.MimeType = file.ContentType;
            //
            // image.Save(picture.GetFullPath("lg"));
            //
            // image.Mutate(i =>
            // i.Resize(new ResizeOptions()
            // {
            // Mode = ResizeMode.Min,
            //  Size = new Size() { Height = 240 }
            // }));
            //
            // image.Save(picture.GetFullPath("sm"));
            //
            // User? user = await _userManager.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            // if (user == null) return Unauthorized();
            //
            // Hub? hub = await _hubService.GetHub(hubId);
            // if (hub == null) return NotFound();
            //
            // Comment? mainComment = await _commentService.CreateComment(user, postDTO.Text, null);
            // if (mainComment == null) return StatusCode(StatusCodes.Status500InternalServerError);
            //
            // Post? post = await _postService.CreatePost(postDTO.Title, hub, mainComment, picture);
            // if (post == null) return StatusCode(StatusCodes.Status500InternalServerError);
            //
            // bool voteToggleSuccess = await _commentService.UpvoteComment(mainComment.Id, user);
            //if (!voteToggleSuccess) return StatusCode(StatusCodes.Status500InternalServerError);
            //
            // return Ok(new PostDisplayDTO(post, true, user));
            // }
            //     catch (Exception ex)
            // {
            // return BadRequest(ex.Message);
            // }
            // }
            // Liste de pict   
            List<Picture> pictures = new List<Picture>();
            var i = 0;
            IFormCollection formCollection = await Request.ReadFormAsync();
            IFormFile? file = formCollection.Files.GetFile("monImage" + i);
            while (file != null) 
            { pictures.Add(await _pictureService.CreateCommentPicture(file, formCollection));
                i++; 
                file = formCollection.Files.GetFile("monImage" + i);
            }            
            //Extraire le texte et le titre du formulaire
            string? title = formCollection["title"]; 
            string? text = formCollection["text"];    
            // Vérifier si les champs requis sont manquants
            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(text)) 
            {            
                return BadRequest("Le titre et le texte sont requis.");   
            }           
            // Obtenir l'utilisateur à partir du contexte actuel
            User? user = await _userManager.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier)!);  
            if (user == null) return Unauthorized();         
            // Obtenir l'utilisateur à partir du contexte actuel
            Hub? hub = await _hubService.GetHub(hubId);  
            if (hub == null) return NotFound();   
            // Créer le commentaire principal
            Comment? mainComment = await _commentService.CreateComment(user, text, null);
            // pictures
            if (mainComment == null) return StatusCode(StatusCodes.Status500InternalServerError);  
            // Créer le post
            Post? post = await _postService.CreatePost(title, hub, mainComment);    
            if (post == null) return StatusCode(StatusCodes.Status500InternalServerError);  
            // Créer le post
            bool voteToggleSuccess = await _commentService.UpvoteComment(mainComment.Id, user); 
            if (!voteToggleSuccess) return StatusCode(StatusCodes.Status500InternalServerError);
            // Retourner le PostDisplayDTO
            return Ok(new PostDisplayDTO(post, true, user));
        }

            /// <summary>
            /// Obtenir une list de posts selon certains critères
            /// </summary>
            /// <param name="tabName">"myHubs" ou "discover"</param>
            /// <param name="sorting">"popular" ou "recent"</param>
            /// <returns>Une liste de PostDisplayDTO pour afficher le commentaire principal de chaque Post</returns>
            [HttpGet("{tabName}/{sorting}")]
        public async Task<ActionResult<IEnumerable<PostDisplayDTO>>> GetPosts(string tabName, string sorting)
        {
            string? userid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            User? user = null;
            if (userid != null) user = await _userManager.FindByIdAsync(userid);

            List<Post> posts = new List<Post>();
            IEnumerable<Hub>? hubs;

            if (tabName == "myHubs" && user != null && user.Hubs != null)
            {
                hubs = user.Hubs;
            }
            else
            {
                hubs = await _hubService.GetAllHubs();
                if (hubs == null) return StatusCode(StatusCodes.Status500InternalServerError);
            }

            int postPerHub = (int)Math.Ceiling(10.0 / hubs.Count());

            foreach (Hub h in hubs)
            {
                if (sorting == "popular") posts.AddRange(GetPopularPosts(h, postPerHub));
                else posts.AddRange(GetRecentPosts(h, postPerHub));
            }

            if (sorting == "popular")
                posts = posts.OrderByDescending(p => p.MainComment?.Upvoters?.Count - p.MainComment?.Downvoters?.Count).ToList();
            else
                posts = posts.OrderByDescending(p => p.MainComment?.Date).ToList();

            return Ok(posts.Select(p => new PostDisplayDTO(p, false, null)));
        }

        /// <summary>
        /// Obtenir une liste de posts à l'aide d'une recherche textuelle.
        /// </summary>
        /// <param name="searchText">Un texte à rechercher dans les titres et dans le texte des commentaires principaux.</param>
        /// <param name="sorting">"popular" ou "recent"</param>
        /// <returns>Une liste de PostDisplayDTO pour afficher les posts avec leur commentaire principal.</returns>
        [HttpGet("{searchText}/{sorting}")]
        public async Task<ActionResult<IEnumerable<PostDisplayDTO>>> SearchPosts(string searchText, string sorting)
        {
            List<Post> posts = new List<Post>();
            IEnumerable<Hub>? hubs = await _hubService.GetAllHubs();
            if (hubs == null) return StatusCode(StatusCodes.Status500InternalServerError);

            foreach (Hub h in hubs)
            {
                h.Posts ??= new List<Post>();
                posts.AddRange(h.Posts.Where(p => p.MainComment!.Text.ToUpper().Contains(searchText.ToUpper()) || p.Title.ToUpper().Contains(searchText.ToUpper())));
            }

            if (sorting == "popular")
                posts = posts.OrderByDescending(p => p.MainComment?.Upvoters?.Count - p.MainComment?.Downvoters?.Count).ToList();
            else
                posts = posts.OrderByDescending(p => p.MainComment?.Date).ToList();

            return Ok(posts.Select(p => new PostDisplayDTO(p, false, null)));
        }

        // Obtenir les Posts d'un Hub par son id. sorting vaut "popular" ou "recent".
        [HttpGet("{hubId}/{sorting}")]
        public async Task<ActionResult<IEnumerable<PostDisplayDTO>>> GetHubPosts(int hubId, string sorting)
        {
            Hub? hub = await _hubService.GetHub(hubId);
            if (hub == null) return NotFound();

            IEnumerable<PostDisplayDTO>? posts = hub.Posts?.Select(p => new PostDisplayDTO(p, false, null));
            if (sorting == "popular") posts = posts?.OrderByDescending(p => p.MainComment.Upvotes - p.MainComment.Downvotes);
            else posts = posts?.OrderByDescending(p => p.MainComment.Date);

            return Ok(posts);
        }

        // Obtenir un Post entier, incluant son commentaire principal et tous ses sous-commentaires.
        [HttpGet("{postId}/{sorting}")]
        public async Task<ActionResult<PostDisplayDTO>> GetFullPost(int postId, string sorting)
        {
            string? userid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            User? user = null;
            if (userid != null) user = await _userManager.FindByIdAsync(userid);

            Post? post = await _postService.GetPost(postId);
            if (post == null) return NotFound();

            PostDisplayDTO postDisplayDTO = new PostDisplayDTO(post, true, user);
            if (sorting == "popular")
                postDisplayDTO.MainComment.SubComments = postDisplayDTO.MainComment!.SubComments!.OrderByDescending(c => c.Upvotes - c.Downvotes).ToList();
            else
                postDisplayDTO.MainComment.SubComments = postDisplayDTO.MainComment!.SubComments!.OrderByDescending(c => c.Date).ToList();

            return Ok(postDisplayDTO);
        }

        // Obtenir les Posts dont le commentaire principal a le plus d'upvotes
        private static IEnumerable<Post> GetPopularPosts(Hub hub, int qty)
        {
            return hub.Posts!.OrderByDescending(p => p.MainComment?.Upvoters?.Count - p.MainComment?.Downvoters?.Count).Take(qty);
        }

        // Obtenir les Posts dont le commentaire principal est le plus récent
        private static IEnumerable<Post> GetRecentPosts(Hub hub, int qty)
        {
            return hub.Posts!.OrderByDescending(p => p.MainComment?.Date).Take(qty);
        }
    }
}
