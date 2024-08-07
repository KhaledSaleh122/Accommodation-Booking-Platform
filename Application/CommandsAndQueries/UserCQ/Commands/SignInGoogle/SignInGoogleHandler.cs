using Application.Dtos.UserDtos;
using Application.Execptions;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;

namespace Application.CommandsAndQueries.UserCQ.Commands.SignInGoogle
{
    internal class SignInGoogleHandler : IRequestHandler<SignInGoogleCommand, UserSignInDto>
    {
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;

        public SignInGoogleHandler(UserManager<User> userManager, IConfiguration configuration)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }
        public async Task<UserSignInDto> Handle(SignInGoogleCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var username = request.Claims.Where(x => x.Type == ClaimTypes.Name).First().Value;
                var name = string.Empty;
                foreach (var letter in username)
                {
                    if (Char.IsLetterOrDigit(letter)) name += letter;
                }
                var email = request.Claims.Where(x => x.Type == ClaimTypes.Email).First().Value;
                var id = request.Claims.Where(x => x.Type == ClaimTypes.NameIdentifier).First().Value;
                var user = new User() { Id = id, Email = email, UserName = name, Thumbnail = "user.jpg" };
                var userAccount = await _userManager.FindByIdAsync(user.Id);
                var userRoles = await _userManager.GetRolesAsync(user);
                if (userAccount is null)
                {
                    var userByEmail = await _userManager.FindByEmailAsync(user.Email);
                    if (userByEmail is not null)
                        throw new ErrorException("User with this email already exist")
                        {
                            StatusCode = StatusCodes.Status409Conflict
                        };
                    var result = await _userManager.CreateAsync(user);
                    await _userManager.AddToRoleAsync(user, "User");
                    userRoles.Add("User");
                }

                var signHandler = new SignInTokenHandler(_configuration);
                var tokenInfo = signHandler.SignIn(userAccount ?? user, userRoles);
                return tokenInfo;
            }
            catch (ErrorException)
            {
                throw;
            }
            catch (Exception exception)
            {

                throw new ErrorException("Error during sing in using google", exception);
            }
        }
    }
}
