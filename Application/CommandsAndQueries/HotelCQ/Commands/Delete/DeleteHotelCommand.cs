using Application.Dtos.HotelDtos;
using MediatR;

namespace Application.CommandsAndQueries.HotelCQ.Commands.Delete
{
    public class DeleteHotelCommand: IRequest<HotelMinDto>
    {
        public int Id { get; set; }
    }
}
