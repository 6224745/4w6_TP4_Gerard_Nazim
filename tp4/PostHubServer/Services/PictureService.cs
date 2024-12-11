using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PostHubServer.Data;
using PostHubServer.Models;
using System.Text.RegularExpressions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using Microsoft.AspNetCore.SignalR;


namespace PostHubServer.Services
{
    public class PictureService
    {
        private readonly PostHubContext _context;

        public PictureService(PostHubContext context)
        {
            _context = context;
        }
        public async Task<Picture?> CreateCommentPicture(IFormFile? file)
        {
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
            // À modifier
            if (_context.Pictures == null)
            {
                return null;
            }

            Picture? pic = await _context.Pictures.FindAsync(id);
            return pic;
        }

        private bool IsContextNull() => _context == null || _context.Pictures == null;




        public async Task<ActionResult<IEnumerable<int>>> GetPictureIds(int commentId)
        {
            return await _context.Comments.Where(c => c.Id == commentId)
                .SelectMany(c => c.Pictures)
                .Select(p => p.Id)
                .ToListAsync();
        }



        public async Task<ActionResult<Picture?>> AddPicture(Picture p)
        {
            _context.Pictures.Add(p);
            await _context.SaveChangesAsync();
            return p;
        }

        public async Task<Picture?> GetPictureId(int id)
        {
            Picture? picture = await _context.Pictures.FindAsync(id);

            return picture;
        }

        [HttpDelete("{id}")]
        public async Task DeletePictureById(int id)
        {
            if (_context.Pictures == null)
            {
                return;
            }

            var picture = await GetPictureId(id);

            if (picture == null)
            {
                return;
            }

            if (picture.MimeType != null && picture.FileName != null)
            {
                string thumbnailPath = Directory.GetCurrentDirectory() + "/images/thumbnail/" + picture.FileName;
                string fullImagePath = Directory.GetCurrentDirectory() + "/images/full/" + picture.FileName;

                if (System.IO.File.Exists(thumbnailPath))
                {
                    System.IO.File.Delete(thumbnailPath);
                }

                if (System.IO.File.Exists(fullImagePath))
                {
                    System.IO.File.Delete(fullImagePath);
                }
            }


            _context.Pictures.Remove(picture);
            await _context.SaveChangesAsync();

        }
    }
}
