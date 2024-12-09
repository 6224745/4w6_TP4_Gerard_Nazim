
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PostHubServer.Models;

namespace PostHubServer.Data
{
    public class PostHubContext : IdentityDbContext<User>
        
    {
        public PostHubContext (DbContextOptions<PostHubContext> options) : base(options){}
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<IdentityRole>().HasData(
                new IdentityRole { Id = "1", Name = "admin", NormalizedName = "ADMIN" },
                new IdentityRole { Id = "2", Name = "moderator", NormalizedName = "MODERATOR" }
            );
            PasswordHasher<User> passwordHasher = new PasswordHasher<User>();

            User u1 = new User

            {
                Id="11111111-1111-1111-1111-111111111111",
                UserName = "moderator",
               NormalizedUserName= "MODERATOR",
               Email="a@a.a"
                

            };
            u1.PasswordHash = passwordHasher.HashPassword(u1, "!Canada2022");
            builder.Entity<IdentityUserRole<string>>().HasData(
                new IdentityUserRole<string> { UserId=u1.Id,RoleId="2"}
                );


        }

        public DbSet<Hub> Hubs { get; set; } = default!;
        public DbSet<Comment> Comments { get; set; } = default!;
        public DbSet<Picture> Pictures { get; set; } = default!;
        public DbSet<Post> Posts { get; set; } = default!;
    }
}
