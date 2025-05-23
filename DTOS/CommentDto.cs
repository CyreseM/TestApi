
namespace TestApi.DTOS
{
    public class CommentDto
    {
    

        public Guid Id { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public int Likes { get; internal set; }

        public int DisLikes { get; set; }
       
        public List<CommentDto> Replies { get; set; } = new();
        public Guid UserId { get;  set; }
    }
}
