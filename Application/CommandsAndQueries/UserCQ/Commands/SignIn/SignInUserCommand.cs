using Application.Dtos.UserDtos;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Application.CommandsAndQueries.UserCQ.Commands.SignIn
{
    public class SignInUserCommand : IRequest<UserSignInDto?>
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
