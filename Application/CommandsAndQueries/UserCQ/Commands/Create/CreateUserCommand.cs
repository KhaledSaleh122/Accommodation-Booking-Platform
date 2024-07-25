using Application.Dtos.UserDtos;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.CommandsAndQueries.UserCQ.Commands.Create
{
    public class CreateUserCommand : IRequest<UserDto>
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public IFormFile Thumbnail { get; set; }
    }
}
