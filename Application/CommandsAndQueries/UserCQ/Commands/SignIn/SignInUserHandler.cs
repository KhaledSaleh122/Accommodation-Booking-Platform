using Application.Exceptions;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Application.CommandsAndQueries.UserCQ.Commands.SignIn
{
    public class SignInUserHandler : IRequestHandler<SignInUserCommand, string>
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IConfiguration _configuration;

        public SignInUserHandler(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        public async Task<string> Handle(SignInUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByNameAsync(request.UserName)
                ?? throw new CustomValidationException("Username is worng!");
            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
            if (!result.Succeeded)
                throw new CustomValidationException("Password is worng!");

            var roles = await _userManager.GetRolesAsync(user);

            var tokenhandler = new JwtSecurityTokenHandler();
            var tkey = Encoding.UTF8.GetBytes(_configuration.GetValue<string>("JWTToken:Key")!);
            var TokenDescp = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(
                [
                    new Claim("Username", user.UserName!),
                    new Claim("Email", user.Email!),
                    new Claim("Id", user.Id),
                    new Claim("Roles",String.Join(",",roles)),
                ]),
                Issuer = _configuration.GetValue<string>("JWTToken:Issuer"),
                Audience = _configuration.GetValue<string>("JWTToken:Audience"),
                Expires = DateTime.UtcNow.AddMinutes(60),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(tkey),
                    SecurityAlgorithms.HmacSha256Signature
                )
            };
            var token = tokenhandler.CreateToken(TokenDescp);
            return tokenhandler.WriteToken(token);
        }
    }
}
