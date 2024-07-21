using Application.Dtos.HotelDtos;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.CommandsAndQueries.HotelCQ.Commands.Create
{
    public class CreateHotelCommand : IRequest<HotelMinDto>
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public List<IFormFile> Images { get; set; }
        public IFormFile Thumbnail { get; set; }
        public string Owner { get; set; }
        public string Address { get; set; }
        public int HotelType { get; set; }
        public int CityId { get; set; }

        public decimal PricePerNight { get; set; }

    }
}
