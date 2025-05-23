using System.ComponentModel.DataAnnotations;

namespace TestApi.DTOS
{
    public class CreateReplyDto
    {
        [Required]
        public string Content { get; set; }
    }
}
