
namespace TestApi.DTOS
{
    public class CommentDto
    {
        internal int DisLikes;
        internal List<CommentDto> Replies;

        public Guid Id { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public int Likes { get; internal set; }

        public int Dislikes { get; set; }
    }
}
