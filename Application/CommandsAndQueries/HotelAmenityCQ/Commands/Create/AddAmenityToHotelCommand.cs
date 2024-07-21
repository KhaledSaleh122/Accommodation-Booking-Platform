using MediatR;

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

        public int HotelId { get; set; }
        public int AmenityId { get; set; }
    }
}
