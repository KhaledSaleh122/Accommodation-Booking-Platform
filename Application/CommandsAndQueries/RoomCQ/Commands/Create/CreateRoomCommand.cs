using Application.Dtos.RoomDtos;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.CommandsAndQueries.RoomCQ.Commands.Create
{
    public class CreateRoomCommand : IRequest<RoomDto>
    {
        public int hotelId;
        public string RoomNumber { get; set; }
        public int AdultCapacity { get; set; }
        public int ChildrenCapacity { get; set; }
        public IFormFile Thumbnail { get; set; }
        public List<IFormFile> Images { get; set; }
    }
}
