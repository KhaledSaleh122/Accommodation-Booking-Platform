using Microsoft.IdentityModel.Tokens;

namespace Application.Dtos.UserDtos
{
    public class UserSignInDto
    {
        public string Token { get; set; }
        public DateTime Expiration { get; set; }
    }
}
