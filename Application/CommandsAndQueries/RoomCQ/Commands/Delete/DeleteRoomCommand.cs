using Application.Dtos.RoomDtos;
using MediatR;

namespace Application.CommandsAndQueries.RoomCQ.Commands.Delete
{
    public class DeleteRoomCommand : IRequest<RoomDto>
    {
        public string RoomNumber { get; set; }
        public int HotelId { get; set; }
    }
}
