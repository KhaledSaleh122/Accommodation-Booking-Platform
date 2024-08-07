using Application.Dtos.UserDtos;
using MediatR;
using System.Security.Claims;

namespace Application.CommandsAndQueries.UserCQ.Commands.SignInGoogle
{
    public class SignInGoogleCommand : IRequest<UserSignInDto>
    {
        public SignInGoogleCommand(IEnumerable<Claim> claims)
        {
            Claims = claims;
        }

        public IEnumerable<Claim> Claims { get; set; }
    }
}
