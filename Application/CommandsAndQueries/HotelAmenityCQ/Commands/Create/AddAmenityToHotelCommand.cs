using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Application.CommandsAndQueries.HotelAmenityCQ.Commands.Create
{
    public class AddAmenityToHotelCommand : IRequest
    {
        public AddAmenityToHotelCommand(int hotelId, int amenityId)
        {
            HotelId = hotelId;
            AmenityId = amenityId;
        }

        public AddAmenityToHotelCommand() { }
        [Required]
        public int HotelId { get; set; }
        [Required]
        public int AmenityId { get; set; }
    }
}
