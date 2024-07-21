using Application.Dtos.HotelDtos;
using MediatR;

namespace Application.CommandsAndQueries.HotelCQ.Commands.Delete
{
    public class DeleteHotelCommand(int id) : IRequest<HotelMinDto>
    {
        public int Id { get; set; } = id;
    }
}
