using Domain.Abstractions;
using Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Infrastructure.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;

        public TokenService(IConfiguration configuration) {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }
        public (string Token, DateTime ExpireDate) GenerateUserToken(User user, IEnumerable<string> roles)
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
            return (tokenhandler.WriteToken(token), expireDate );
        }
    }
}
