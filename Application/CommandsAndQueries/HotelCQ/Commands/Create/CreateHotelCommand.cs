using Application.Dtos.HotelDtos;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Application.CommandsAndQueries.HotelCQ.Commands.Create
{
    public class CreateHotelCommand : IRequest<HotelMinDto>
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public List<IFormFile> Images { get; set; }
        [Required]
        public IFormFile Thumbnail { get; set; }
        [Required]
        public string Owner { get; set; }
        [Required]
        public string Address { get; set; }
        [Required]
        public int HotelType { get; set; }
        [Required]
        public int CityId { get; set; }
        [Required]
        public decimal PricePerNight { get; set; }

    }
}
