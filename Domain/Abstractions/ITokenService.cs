using Domain.Entities;

namespace Domain.Abstractions
{
    public interface ITokenService
    {
        (string Token, DateTime ExpireDate) GenerateUserToken(User user, IEnumerable<string> roles);
    }
}
