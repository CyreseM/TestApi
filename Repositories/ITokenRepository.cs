using TestApi.Models;

namespace TestApi.Repositories
{
    public interface ITokenRepository
    {
        string CreateToken(AppUser user, List<string> roles);

    }
}
