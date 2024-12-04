using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using PostHubServer.Models;
using PostHubServer.Models.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using SixLabors.ImageSharp;
using PostHubServer.Services;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using SixLabors.ImageSharp.Processing;

namespace PostHubServer.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        readonly UserManager<User> _userManager;
        private readonly PictureService _pictureService;

        public UsersController(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        [HttpPost]
        public async Task<ActionResult> Register(RegisterDTO register)
        {
            if (register.Password != register.PasswordConfirm)
            {
                return StatusCode(StatusCodes.Status400BadRequest,
                    new { Message = "Les deux mots de passe spécifiés sont différents." });
            }
            User user = new User()
            {
                UserName = register.Username,
                Email = register.Email
            };
            IdentityResult identityResult = await _userManager.CreateAsync(user, register.Password);
            if (!identityResult.Succeeded)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { Message = "La création de l'utilisateur a échoué." });
            }
            return Ok(new { Message = "Inscription réussie ! 🥳" });
        }

        [HttpPost]
        public async Task<ActionResult> Login(LoginDTO login)
        {
            User? user = await _userManager.FindByNameAsync(login.Username);
            if (user != null && await _userManager.CheckPasswordAsync(user, login.Password))
            {
                IList<string> roles = await _userManager.GetRolesAsync(user);
                List<Claim> authClaims = new List<Claim>();
                foreach (string role in roles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, role));
                }
                authClaims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id));
                SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8
                    .GetBytes("LooOOongue Phrase SiNoN Ça ne Marchera PaAaAAAaAas !"));
                JwtSecurityToken token = new JwtSecurityToken(
                    issuer: "https://localhost:7216",
                    audience: "http://localhost:4200",
                    claims: authClaims,
                    expires: DateTime.Now.AddMinutes(300),
                    signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature)
                    );
                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    validTo = token.ValidTo,
                    username = user.UserName // Ceci sert déjà à afficher / cacher certains boutons côté Angular
                });
            }
            else
            {
                return StatusCode(StatusCodes.Status400BadRequest,
                    new { Message = "Le nom d'utilisateur ou le mot de passe est invalide." });
            }
        }
        [HttpPost("{username}")]
        [DisableRequestSizeLimit]
        public async Task<ActionResult> ChangeAvatar(string username)
        {
            try
            {
                User? user = await _userManager.FindByNameAsync(username);
                if (user == null)
                {
                    return NotFound(new { Message = "Utilisateur introuvable." });
                }

                IFormFile? file = Request.Form.Files.GetFile("image");
                if (file == null)
                {
                    return BadRequest(new { Message = "Aucune image fournie." });
                }

                Image image = Image.Load(file.OpenReadStream());
                user.FileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                user.MimeType = file.ContentType;

                string avatarPath = Path.Combine(Directory.GetCurrentDirectory(), "images", "Avatar", user.FileName);
                Directory.CreateDirectory(Path.GetDirectoryName(avatarPath) ?? string.Empty);

                image.Mutate(i => i.Resize(new ResizeOptions
                {
                    Mode = ResizeMode.Min,
                    Size = new Size(200, 200)
                }));
                image.Save(avatarPath);

                IdentityResult result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Échec de la mise à jour de l'utilisateur." });
                }

                return Ok(new { Message = "Avatar modifié avec succès !" });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Erreur serveur.", Error = ex.Message });
            }
        }
        [HttpGet("{username}")]
        public async Task<IActionResult> GetAvatar(string username)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user == null || string.IsNullOrEmpty(user.FileName))
            {
                // Renvoie l'image par défaut
                string defaultAvatarPath = Path.Combine(Directory.GetCurrentDirectory(), "images", "avatar", "default.png");
                if (!System.IO.File.Exists(defaultAvatarPath))
                {
                    return NotFound(new { Message = "Image par défaut introuvable." });
                }
                byte[] defaultImage = await System.IO.File.ReadAllBytesAsync(defaultAvatarPath);
                return File(defaultImage, "image/png");
            }

            string avatarPath = Path.Combine(Directory.GetCurrentDirectory(), "images", "avatar", user.FileName);
            if (System.IO.File.Exists(avatarPath))
            {
                byte[] image = await System.IO.File.ReadAllBytesAsync(avatarPath);
                return File(image, user.MimeType);
            }

            // Fallback vers l'image par défaut si l'image utilisateur est manquante
            string fallbackAvatarPath = Path.Combine(Directory.GetCurrentDirectory(), "images", "avatar", "default.png");
            if (!System.IO.File.Exists(fallbackAvatarPath))
            {
                return NotFound(new { Message = "Image par défaut introuvable." });
            }
            byte[] fallbackImage = await System.IO.File.ReadAllBytesAsync(fallbackAvatarPath);
            return File(fallbackImage, "image/png");
        }
    }

}

