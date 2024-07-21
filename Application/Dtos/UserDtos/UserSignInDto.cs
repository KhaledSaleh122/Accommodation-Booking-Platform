using Microsoft.IdentityModel.Tokens;

namespace Application.Dtos.UserDtos
{
    public class UserSignInDto
    {
        public SecurityToken Token { get; set; }
    }
}
