namespace TestApi.DTOS
{
    public class CommentReplyDto
    {
        public Guid Id { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid? ParentCommentId { get; set; }
        public Guid PostId { get; set; }
        public Guid UserId { get; internal set; }
    }
}
