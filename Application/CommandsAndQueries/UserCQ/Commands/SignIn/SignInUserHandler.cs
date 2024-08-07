using Application.Dtos.UserDtos;
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
    internal class SignInUserHandler : IRequestHandler<SignInUserCommand, UserSignInDto?>
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

        public async Task<UserSignInDto?> Handle(SignInUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByNameAsync(request.UserName);
            if (user is null) return null;
            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
            if (!result.Succeeded)
                return null;

            var roles = await _userManager.GetRolesAsync(user);

            var tokenHandler = new SignInTokenHandler(_configuration);
            return tokenHandler.SignIn(user, roles);
        }
    }
}
