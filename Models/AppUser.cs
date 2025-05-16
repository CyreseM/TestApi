using Microsoft.AspNetCore.Identity;

namespace TestApi.Models
{
    public class AppUser: IdentityUser
    {
        public ICollection<Post> Posts { get; set; }
        public ICollection<Comment> Comments { get; set; }
    }
}
