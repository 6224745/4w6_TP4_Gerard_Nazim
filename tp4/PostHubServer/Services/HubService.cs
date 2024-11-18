using Microsoft.EntityFrameworkCore;
using PostHubServer.Data;
using PostHubServer.Models;

namespace PostHubServer.Services
{
    public class HubService
    {
        private readonly PostHubContext _context;

        public HubService(PostHubContext context)
        {
            _context = context;
        }

        // Obtenir la liste des hubs rejoints par un utilisateur
        public IEnumerable<Hub>? GetUserHubs(User user)
        {
            if (IsContextNull()) return null;

            if (user.Hubs == null) return new List<Hub>(); // L'utilisateur n'a rejoint aucun Hub

            return user.Hubs;
        }

        // Obtenir un hub spécifique par son id
        public async Task<Hub?> GetHub(int id)
        {
            if (IsContextNull()) return null;

            return await _context.Hubs.FindAsync(id);
        }

        // Ajouter / retirer un hub parmi les hubs de l'utilisateur
        public async Task<Hub?> ToggleJoinHub(int id, User user)
        {
            if (IsContextNull()) return null;

            Hub? hub = await _context.Hubs.FindAsync(id);
            if (hub == null) return null;

            hub.Users ??= new List<User>();

            if (hub.Users.Contains(user)) hub.Users.Remove(user);
            else hub.Users.Add(user);
            await _context.SaveChangesAsync();

            return hub;
        }

        // Obtenir tous les hubs
        public async Task<IEnumerable<Hub>?> GetAllHubs()
        {
            if (IsContextNull()) return null;

            return await _context.Hubs.ToListAsync();
        }

        // Créer un nouvel hub
        public async Task<Hub?> CreateHub(Hub hub)
        {
            if (IsContextNull()) return null;

            _context.Hubs.Add(hub);
            await _context.SaveChangesAsync();
            return hub;
        }

        private bool IsContextNull() => _context == null || _context.Hubs == null;
    }
}
