
// CommentDto is used to return comments with nested replies.
// Consider adding author display name, avatar, and per-user reaction state.
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
        public string UserId { get;  set; }
    }
}
