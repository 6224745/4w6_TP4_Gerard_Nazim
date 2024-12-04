using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PostHubServer.Data;
using PostHubServer.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.Text.RegularExpressions;

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

            Image image = Image.Load(file.OpenReadStream());
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
                    Size = new Size() { Width = 320 }
                })
            );
            image.Save(Directory.GetCurrentDirectory() + "/images/thumbnail/" + picture.FileName);

            _context.Pictures.Add(picture);
            await _context.SaveChangesAsync();
            return picture;
        }

        public async Task<Picture> GetCommentPicture(int id)
        {
            Picture? pic = await _context.Pictures.FindAsync(id);
            return pic;
        }
        public async Task<bool> DeletePictureAsync(int id)
        {
            var picture = await _context.Pictures.FindAsync(id);
            if (picture == null)
            {
                return false;
            }

            System.IO.File.Delete(Directory.GetCurrentDirectory() + "/images/full/" + picture.FileName);
            System.IO.File.Delete(Directory.GetCurrentDirectory() + "/images/thumbnail/" + picture.FileName);

            _context.Pictures.Remove(picture);
            await _context.SaveChangesAsync();

            return true;
        }

        private bool IsContextNull() => _context == null || _context.Pictures == null;
    }
}
