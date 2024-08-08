using Application.Dtos.HotelDtos;
using MediatR;

namespace Application.CommandsAndQueries.CityCQ.Commands.Update
{
    public class UpdateHotelCommand : IRequest<HotelMinDto>
    {
        public int hotelId;
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Owner { get; set; }
        public string? Address { get; set; }

        public decimal? PricePerNight { get; set; }
        public int? HotelType { get; set; }
    }
}
