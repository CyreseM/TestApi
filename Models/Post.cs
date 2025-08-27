namespace TestApi.Models
{
    public class Post
    {
        public Guid Id { get; set; }

        public string Title { get; set; }
        public string Slug { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int Likes { get; set; } = 0;
        public int Dislikes { get; set; } = 0;
        // Comma-separated tags for simplicity. Consider a join table for richer tag queries.
        public string TagsCsv { get; set; }

        public string UserId { get; set; }
        public AppUser User { get; set; }
        public string UserName { get; set; }
        
        public ICollection<Comment> Comments { get; set; }

                               
    }
}
