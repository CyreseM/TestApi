using TestApi.Models;

namespace TestApi.DTOS
{
    public class CreatePostDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
 
        public string UserName { get; internal set; }

        public Guid UserId { get; internal set; }
        public int Likes { get; internal set; } = 0;
        public int DisLikes { get; internal set; } = 0;
        public List<CommentDto> Comments { get; internal set; }
    }
}
