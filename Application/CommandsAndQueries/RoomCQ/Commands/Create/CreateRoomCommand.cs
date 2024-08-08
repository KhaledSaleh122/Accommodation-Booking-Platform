using Application.Dtos.RoomDtos;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Application.CommandsAndQueries.RoomCQ.Commands.Create
{
    public class CreateRoomCommand : IRequest<RoomDto>
    {
        public int hotelId;
        [Required]
        public string RoomNumber { get; set; }
        [Required]
        public int AdultCapacity { get; set; }
        [Required]
        public int ChildrenCapacity { get; set; }
        [Required]
        public IFormFile Thumbnail { get; set; }
        [Required]
        public List<IFormFile> Images { get; set; }
    }
}
