using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ABP.Presentation.IntegrationTests
{
    public class JwtTokenHelper
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<User> _userManager;
        private readonly byte[] _tokenKey;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly JwtSecurityTokenHandler _tokenHandler;

        public JwtTokenHelper(IConfiguration configuration, UserManager<User> userManager)
        {
            _configuration = configuration;
            _userManager = userManager;
            _tokenKey = Encoding.UTF8.GetBytes(_configuration.GetValue<string>("JWTToken:Key")!);
            _issuer = _configuration.GetValue<string>("JWTToken:Issuer")!;
            _audience = _configuration.GetValue<string>("JWTToken:Audience")!;
            _tokenHandler = new();
        }
        public async Task<string> GetJwtTokenAsync(string username)
        {
            var user = await _userManager.FindByNameAsync(username) ?? throw new ArgumentNullException("Account not found");
            var claims = new List<Claim>() {
                new (ClaimTypes.Name, user.UserName!),
                new (ClaimTypes.Email, user.Email!),
                new (ClaimTypes.NameIdentifier, user.Id),
            };
            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                claims.Add(new(ClaimTypes.Role, role));
            }
            var expireDate = DateTime.UtcNow.AddMinutes(60);
            var TokenDescp = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Issuer = _issuer,
                Audience = _audience,
                Expires = DateTime.UtcNow.AddMinutes(60),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(_tokenKey),
                    SecurityAlgorithms.HmacSha256Signature
                )
            };
            var token = _tokenHandler.CreateToken(TokenDescp);
            return _tokenHandler.WriteToken(token);
        }
    }
}
