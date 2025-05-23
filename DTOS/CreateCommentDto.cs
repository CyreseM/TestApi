using System.ComponentModel.DataAnnotations;

namespace TestApi.DTOS
{
    public class CreateCommentDto
    {
        [Required]
        public string Content { get; set; }
        public Guid UserId { get; set; }
    }
}
