using PostHubServer.Data;
using PostHubServer.Models;

using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Drawing;
using Image = SixLabors.ImageSharp.Image;

namespace PostHubServer.Services
{
    public class PictureService
    {
        private readonly PostHubContext _context;

        public PictureService(PostHubContext context)
        {
            _context = context;
        }

        public async Task<Picture?> CreateCommentPicture(IFormFile? file, IFormCollection formcollection)
        {
            if (IsContextNull()) return null;

           Image? image = Image.Load(file.OpenReadStream());
            Picture picture = new Picture
            {
                Id = 0,
                FileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName),
                MimeType = file.ContentType
            };

            image.Save(Directory.GetCurrentDirectory() + "/images/full/" + picture.FileName);
            image.Mutate(i =>
                i.Resize(new ResizeOptions()
                {
                    Mode = ResizeMode.Min,
                    Size = new SixLabors.ImageSharp.Size() { Width = 320 }
                })
            );
            image.Save(Directory.GetCurrentDirectory() + "/images/thumbnail/" + picture.FileName);

            return picture;
        }

        private bool IsContextNull() => _context == null || _context.Pictures == null;
    }
}