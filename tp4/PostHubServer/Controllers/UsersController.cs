using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using PostHubServer.Models;
using PostHubServer.Models.DTOs;
using PostHubServer.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
//using System.Drawing;

namespace PostHubServer.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        readonly UserManager<User> _userManager;
        readonly PictureService _pictureService;

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
            if (user == null)
            {
                user = await _userManager.FindByEmailAsync(login.Username);
            }
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
                    username = user.UserName, // Ceci sert déjà à afficher / cacher certains boutons côté Angular
                    role = roles// Send the roles array back to the frontend
                });
            }
            else
            {
                return StatusCode(StatusCodes.Status400BadRequest,
                    new { Message = "Le nom d'utilisateur ou le mot de passe est invalide." });
            }
        }
        [HttpPut]
        public async Task<ActionResult> ChangeAvatar()
        {
            IFormCollection formCollection = await Request.ReadFormAsync();
            string userName = formCollection["username"];

            if (string.IsNullOrEmpty(userName))
            {
                return BadRequest("The userName field is required.");
            }

            User user = await _userManager.FindByNameAsync(userName);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            IFormFile? file = formCollection.Files.GetFile("Avatar");
            if (file == null)
            {
                return BadRequest("Avatar file is required.");
            }

            user.FileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            user.MimeType = file.ContentType;

            try
            {

                string avatarDirectory = Path.Combine(Directory.GetCurrentDirectory(), "images", "avatar");
                Directory.CreateDirectory(avatarDirectory);


                string filePath = Path.Combine(avatarDirectory, user.FileName);


                using (var stream = file.OpenReadStream())
                using (var image = Image.Load(stream))
                {
                    image.Mutate(x => x.Resize(new ResizeOptions
                    {
                        Mode = ResizeMode.Max,
                        Size = new Size(200, 200)
                    }));

                    image.Save(filePath);
                }


                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    return BadRequest("Failed to update user data.");
                }
            }
            catch (Exception ex)
            {
                return BadRequest($"Invalid image file: {ex.Message}");
            }


            string avatarUrl = $"/images/avatar/{user.FileName}";
            return Ok(new { AvatarUrl = avatarUrl });
        }



        [HttpGet("{username}")]
        public async Task<ActionResult> GetAvatar(string username)
        {
            try
            {

                User user = await _userManager.FindByNameAsync(username);


                if (user == null)
                {
                    return BadRequest("User not found");
                }




                if (string.IsNullOrEmpty(user.FileName))
                {
                    string defaultPath = Path.Combine(Directory.GetCurrentDirectory(), "images", "avatar", "default.png");


                    if (!System.IO.File.Exists(defaultPath))
                    {
                        return NotFound("Default avatar image is missing.");
                    }

                    byte[] defaultBytes = System.IO.File.ReadAllBytes(defaultPath);
                    return File(defaultBytes, "image/png");
                }


                byte[] avatarBytes = System.IO.File.ReadAllBytes(Directory.GetCurrentDirectory() + "/images/" + "avatar/" + user.FileName);
                return File(avatarBytes, user.MimeType);
            }
            catch (Exception ex)
            {

                Console.WriteLine($"Error fetching avatar for {username}: {ex.Message}");
                return StatusCode(500, "An error occurred while fetching the avatar.");
            }
        }
        [HttpPut]
        public async Task<ActionResult> ChangePassword()
        {
            // Lire les données envoyées dans le FormData
            var formCollection = await Request.ReadFormAsync();
            string oldPassword = formCollection["oldPassword"];
            string newPassword = formCollection["newPassword"];
            string newPasswordConfirm = formCollection["newPasswordConfirm"];
            if (newPassword != newPasswordConfirm)

                // Vérifier que l'ancien et le nouveau mot de passe diffèrent
                if (oldPassword == newPassword)
                {
                    return BadRequest(new { Message = "L'ancien mot de passe est le même que le nouveau" });
                }
            // Récupére l'utilisateur connecté
            User? user = await _userManager.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            if (user == null)
            {
                return Unauthorized(new { Message = "Utilisateur introuvable." });
            }
            // Vérifier que l'ancien et le nouveau mot de passe diffèrent
            if (oldPassword == newPassword)
            {
                return BadRequest(new { Message = "L'ancien mot de passe est le même que le nouveau" });
            }

            // Vérifier si l'ancien mot de passe est correct
            var checkPasswordResult = await _userManager.CheckPasswordAsync(user, oldPassword);
            if (!checkPasswordResult)
            {
                return BadRequest(new { Message = "L'ancien mot de passe est incorrect." });
            }

            // Modifier le mot de passe
            var result = await _userManager.ChangePasswordAsync(user, oldPassword, newPassword);
            if (!result.Succeeded)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Échec du changement de mot de passe." });
            }
            return Ok(new { Message = "Mot de passe modifié avec succès." });
        }
        [HttpPut("{username}")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult> MakeModerator(string username)
        {
            User? newMod = await _userManager.FindByNameAsync(username);
            if (newMod == null)
            {
                return NotFound(new { Message = "Cet utilisateur n'existe pas. " });
            }
            await _userManager.AddToRoleAsync(newMod, "moderator");
            return Ok(new { Message = username + " est maintenant modérateur ! 👑" });
        }

    }
}


