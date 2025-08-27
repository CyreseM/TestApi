namespace TestApi.Models
{
    public enum ReactionState
    {
        None = 0,
        Like = 1,
        Dislike = 2
    }

    public class PostReaction
    {
        public Guid Id { get; set; }
        public Guid PostId { get; set; }
        public string UserId { get; set; }
        public ReactionState State { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}

