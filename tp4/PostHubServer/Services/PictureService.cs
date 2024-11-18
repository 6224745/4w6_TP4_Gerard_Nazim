using PostHubServer.Data;

namespace PostHubServer.Services
{
    public class PictureService
    {
        private readonly PostHubContext _context;

        public PictureService(PostHubContext context)
        {
            _context = context;
        }

        private bool IsContextNull() => _context == null || _context.Pictures == null;
    }
}
