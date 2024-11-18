
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PostHubServer.Data;
using PostHubServer.Models;
using PostHubServer.Models.DTOs;
using PostHubServer.Services;
using System.Security.Claims;

namespace PostHubServer.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class HubsController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly HubService _hubService;

        public HubsController(HubService hubService, UserManager<User> userManager)
        {
            _hubService = hubService;
            _userManager = userManager;
        }

        // Obtenir la liste des hubs rejoints par un utilisateur
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<HubDisplayDTO>>> GetUserHubs()
        {
            User? user = await _userManager.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            if (user == null) return Unauthorized();

            IEnumerable<Hub>? userHubs = _hubService.GetUserHubs(user);
            if (userHubs == null) return StatusCode(StatusCodes.Status500InternalServerError);

            return Ok(userHubs.Select(h => new HubDisplayDTO()
            {
                Id = h.Id,
                Name = h.Name,
                IsJoined = h.Users?.Contains(user)
            }));
        }

        // Créer un nouveau hub
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<HubDisplayDTO>> PostHub(Hub hub)
        {
            User? user = await _userManager.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            if (user == null) return Unauthorized();

            Hub? newHub = await _hubService.CreateHub(hub);
            if (newHub == null) return StatusCode(StatusCodes.Status500InternalServerError);

            Hub? joinedHub = await _hubService.ToggleJoinHub(newHub.Id, user);
            if (joinedHub == null) return StatusCode(StatusCodes.Status500InternalServerError);

            return Ok(new HubDisplayDTO()
            {
                Id = joinedHub.Id,
                Name = joinedHub.Name,
                IsJoined = joinedHub.Users?.Contains(user)
            });
        }

        // Obtenir un hub spécifique par son id
        [HttpGet("{id}")]
        public async Task<ActionResult<HubDisplayDTO>> GetHub(int id)
        {
            string? userid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            User? user = null;
            if (userid != null) await _userManager.FindByIdAsync(userid);

            Hub? hub = await _hubService.GetHub(id);
            if (hub == null) return NotFound();

            return Ok(new HubDisplayDTO()
            {
                Id = hub.Id,
                Name = hub.Name,
                IsJoined = user == null ? null : hub.Users.Contains(user)
            });
        }

        // Permet à un utilisateur d'ajouter / retirer un hub de sa liste de hubs
        [HttpPut("{hubId}")]
        [Authorize]
        public async Task<ActionResult> ToggleJoinHub(int hubId)
        {
            User? user = await _userManager.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            if (user == null) return BadRequest();

            Hub? hub = await _hubService.ToggleJoinHub(hubId, user);
            if (hub == null) return NotFound();

            return Ok(new { Message = hub.Users!.Contains(user) ? "Hub rejoint." : "Hub quitté." });
        }
    }
}
