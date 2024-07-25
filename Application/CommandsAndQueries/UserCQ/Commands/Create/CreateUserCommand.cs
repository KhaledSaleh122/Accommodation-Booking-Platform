using Application.Dtos.UserDtos;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Application.CommandsAndQueries.UserCQ.Commands.Create
{
    public class CreateUserCommand : IRequest<UserDto>
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public IFormFile Thumbnail { get; set; }
    }
}
