using MediatR;

namespace Application.CommandsAndQueries.HotelAmenityCQ.Commands.Delete
{
    public class RemoveAmenityFromHotelCommand : IRequest
    {
        public RemoveAmenityFromHotelCommand()
        {
        }

        public RemoveAmenityFromHotelCommand(int hotelId, int amenityId)
        {
            HotelId = hotelId;
            AmenityId = amenityId;
        }

        public int HotelId { get; set; }
        public int AmenityId { get; set; }
    }
}
