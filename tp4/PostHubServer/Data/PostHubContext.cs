
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PostHubServer.Models;

namespace PostHubServer.Data
{
    public class PostHubContext : IdentityDbContext<User>
    {
        public PostHubContext (DbContextOptions<PostHubContext> options) : base(options){}

        public DbSet<Hub> Hubs { get; set; } = default!;
        public DbSet<Comment> Comments { get; set; } = default!;
        public DbSet<Picture> Pictures { get; set; } = default!;
        public DbSet<Post> Posts { get; set; } = default!;
    }
}
