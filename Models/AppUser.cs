using Microsoft.AspNetCore.Identity;

namespace TestApi.Models
{
    public class AppUser: IdentityUser
    {
        public UserProfile Profile { get; set; }
        public ICollection<Post> Posts { get; set; }
        public ICollection<Comment> Comments { get; set; }
    }
}
