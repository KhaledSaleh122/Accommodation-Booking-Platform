using Application.Dtos.BookingDtos;

namespace Application.Dtos.RoomDtos
{
    public class RoomWithBookingDto
    {
        public string RoomNumber { get; set; }
        public int AdultCapacity { get; set; }
        public int ChildrenCapacity { get; set; }
        public string Thumbnail { get; set; }
        public List<string> Images { get; set; }
        public ICollection<BookingRoomDto> Bookings { get; set; }

    }
}
