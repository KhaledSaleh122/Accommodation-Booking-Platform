using Application.Dtos.UserDtos;
using Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Application.CommandsAndQueries.UserCQ.Commands
{
    public class SignInTokenHandler
    {
        private readonly IConfiguration _configuration;
        public SignInTokenHandler(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public UserSignInDto SignIn(User user, IList<String> roles)
        {
            var tokenhandler = new JwtSecurityTokenHandler();
            var tkey = Encoding.UTF8.GetBytes(_configuration.GetValue<string>("JWTToken:Key")!);
            var claims = new List<Claim>() {
                new (ClaimTypes.Name, user.UserName!),
                new (ClaimTypes.Email, user.Email!),
                new (ClaimTypes.NameIdentifier, user.Id),
            };
            foreach (var role in roles)
            {
                claims.Add(new(ClaimTypes.Role, role));
            }
            var expireDate = DateTime.UtcNow.AddMinutes(60);
            var TokenDescp = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Issuer = _configuration.GetValue<string>("JWTToken:Issuer"),
                Audience = _configuration.GetValue<string>("JWTToken:Audience"),
                Expires = DateTime.UtcNow.AddMinutes(60),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(tkey),
                    SecurityAlgorithms.HmacSha256Signature
                )
            };
            var token = tokenhandler.CreateToken(TokenDescp);
            return new UserSignInDto() { Token = tokenhandler.WriteToken(token), Expiration = expireDate };
        }
    }
}
