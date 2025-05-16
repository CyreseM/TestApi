namespace TestApi.Models
{
    public class Comment
    {
       public Guid Id { get; set; }
       public string Content { get; set; }

       public DateTime CreatedAt { get; set; } = DateTime.Now;

       public int Likes { get; set; } = 0;

       public int Dislikes { get; set; } = 0;
      public Guid? ParentCommentId { get; set; }
      public Comment ParentComment { get; set; }
      public Guid PostId { get; set; }
      public Post Post { get; set; }
      public Guid UserId { get; set; }
      public AppUser User { get; set; }

      public ICollection<Comment> Replies { get; set; }

    }
}
