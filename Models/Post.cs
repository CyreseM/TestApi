namespace TestApi.Models
{
    public class Post
    {
        public Guid Id { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int Likes { get; set; } = 0;
        public int Dislikes { get; set; } = 0;

        public Guid UserId { get; set; }
        public AppUser User { get; set; }
        public string UserName { get; set; }
        
        public ICollection<Comment> Comments { get; set; }


                               
    }
}
