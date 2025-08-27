using TestApi.Models;

// CreatePostDto doubles as a lightweight response model.
// For a stricter design, split into PostCreateRequest and PostResponse DTOs.
using System.ComponentModel.DataAnnotations;
namespace TestApi.DTOS
{
    public class CreatePostDto
    {
        [Required]
        [MaxLength(160)]
        public string Title { get; set; }
        public string Slug { get; set; }
        [MaxLength(300)]
        public string Description { get; set; }
        [Url]
        [MaxLength(500)]
        public string ImageUrl { get; set; }
        [Required]
        public string Content { get; set; }
        public List<string> Tags { get; set; }
        // Response-only fields; ignored on create requests
        public DateTime CreatedAt { get; set; }
        public string UserName { get; set; }
        public string UserId { get; set; }
        public int Likes { get; set; }
        public int DisLikes { get; set; }
        public List<CommentDto> Comments { get; set; }
    }
}
