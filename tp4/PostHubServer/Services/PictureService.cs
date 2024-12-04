<<<<<<< HEAD
﻿using PostHubServer.Data;
using PostHubServer.Models;
using SixLabors.ImageSharp;

using SixLabors.ImageSharp.Processing;
=======
﻿using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using PostHubServer.Data;
using PostHubServer.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.Text.RegularExpressions;
>>>>>>> origin/dev

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
<<<<<<< HEAD

            Image image = Image.Load(file.OpenReadStream());
=======
            Image? image = Image.Load(file.OpenReadStream());
>>>>>>> origin/dev
            Picture picture = new Picture
            {
                Id = 0,
                FileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName),
                MimeType = file.ContentType
            };
<<<<<<< HEAD

=======
>>>>>>> origin/dev
            image.Save(Directory.GetCurrentDirectory() + "/images/full/" + picture.FileName);
            image.Mutate(i =>
                i.Resize(new ResizeOptions()
                {
                    Mode = ResizeMode.Min,
<<<<<<< HEAD
                    Size = new Size() { Width = 50 }
                })
            );
            image.Save(Directory.GetCurrentDirectory() + "/images/thumbnail/" + picture.FileName);

=======
                    Size = new Size() { Width = 320 }
                })
            );
            image.Save(Directory.GetCurrentDirectory() + "/images/thumbnail/" + picture.FileName);
>>>>>>> origin/dev
            _context.Pictures.Add(picture);
            await _context.SaveChangesAsync();
            return picture;
        }
        public async Task<Picture> GetCommentPicture(int id)
        {
<<<<<<< HEAD
=======
            // À modifier
            if (_context.Pictures == null)
            {
                return null;
            }
>>>>>>> origin/dev
            Picture? pic = await _context.Pictures.FindAsync(id);
            return pic;
        }
        private bool IsContextNull() => _context == null || _context.Pictures == null;
    }
}
