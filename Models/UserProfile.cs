using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace TestApi.Models
{
    public class UserProfile
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public AppUser User { get; set; }

        [MaxLength(100)]
        public string DisplayName { get; set; }

        [MaxLength(500)]
        public string Bio { get; set; }

        [MaxLength(255)]
        public string ProfilePictureUrl { get; set; }

        [MaxLength(255)]
        public string Website { get; set; }

        [MaxLength(100)]
        public string Location { get; set; }

        [MaxLength(100)]
        public string TwitterHandle { get; set; }

        [MaxLength(100)]
        public string LinkedInHandle { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
