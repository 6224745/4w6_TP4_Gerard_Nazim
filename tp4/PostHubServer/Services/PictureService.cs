using PostHubServer.Data;
using PostHubServer.Models;

namespace PostHubServer.Services
{
    public class PictureService
    {
        private readonly PostHubContext _context;

        public PictureService(PostHubContext context)
        {
            _context = context;
        }

        internal async Task<Picture> CreateCommentPicture(IFormFile file, IFormCollection formCollection)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("Le fichier est invalide.");

            // Définir le répertoire de stockage
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "comments");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            // Générer un nom unique pour le fichier
            var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            var filePath = Path.Combine(uploadsFolder, fileName);
            var mimeType = file.ContentType;

            // Enregistrer le fichier sur le disque
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Créer l'objet Picture et remplir ses propriétés
            var picture = new Picture
            {
                FileName = fileName,
                MimeType = mimeType
            };

            // Enregistrer la photo en base de données (si nécessaire)
            _context.Pictures.Add(picture);
            await _context.SaveChangesAsync();

            return picture;
        }

        private bool IsContextNull() => _context == null || _context.Pictures == null;
    }
}
