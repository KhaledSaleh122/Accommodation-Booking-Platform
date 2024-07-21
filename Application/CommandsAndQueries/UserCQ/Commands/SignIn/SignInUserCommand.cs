using Application.Dtos.UserDtos;
using MediatR;
using Microsoft.IdentityModel.Tokens;

namespace Application.CommandsAndQueries.UserCQ.Commands.SignIn
{
    public class SignInUserCommand : IRequest<string>
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}
