namespace TestApi.Models
{
    public class CommentReaction
    {
        public Guid Id { get; set; }
        public Guid CommentId { get; set; }
        public string UserId { get; set; }
        public ReactionState State { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}

