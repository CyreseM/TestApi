using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TestApi.Models;

namespace TestApi.Data
{
    public class TestDbContext: IdentityDbContext<AppUser>
    {
        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
        {
        }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Comment> Comments { get; set; }

        public DbSet<UserProfile> Profiles { get; set; }
        public DbSet<PostReaction> PostReactions { get; set; }
        public DbSet<CommentReaction> CommentReactions { get; set; }
        public DbSet<Bookmark> Bookmarks { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            var readerRoleId = "395c11d9-2563-460e-9204-c74191246795";
            var writerRoleId = "6fec2877-aa5d-4a16-bcb1-80a64e277a89";

            var roles = new List<IdentityRole>
                {
                    new IdentityRole()
                    {
                        Id = readerRoleId,
                        ConcurrencyStamp = readerRoleId,
                        Name = "Reader",
                        NormalizedName = "Reader".ToUpper()
                    },
                    new IdentityRole
                    {
                        Id = writerRoleId,
                        ConcurrencyStamp = writerRoleId,
                        Name = "Writer",
                        NormalizedName="writer".ToUpper()
                    }
                };
            modelBuilder.Entity<IdentityRole>().HasData(roles);

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.ParentComment)
                .WithMany(c => c.Replies)
                .HasForeignKey(c => c.ParentCommentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Post)
                .WithMany(p => p.Comments)
                .HasForeignKey(c => c.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            // Ensure 1 reaction per user per post
            modelBuilder.Entity<PostReaction>()
                .HasIndex(r => new { r.PostId, r.UserId })
                .IsUnique();

            // Ensure 1 reaction per user per comment
            modelBuilder.Entity<CommentReaction>()
                .HasIndex(r => new { r.CommentId, r.UserId })
                .IsUnique();

            // Ensure 1 bookmark per user per post
            modelBuilder.Entity<Bookmark>()
                .HasIndex(b => new { b.PostId, b.UserId })
                .IsUnique();

        }
    }
 
}
