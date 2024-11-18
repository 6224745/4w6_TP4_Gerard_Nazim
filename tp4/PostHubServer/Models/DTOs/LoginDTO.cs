using System.ComponentModel.DataAnnotations;

namespace PostHubServer.Models.DTOs
{
    public class LoginDTO
    {
        [Required]
        public string Username { get; set; } = null!;
        [Required]
        public string Password { get; set; } = null!;
    }
}
