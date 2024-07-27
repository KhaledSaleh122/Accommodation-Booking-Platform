using Domain.Enums;

namespace Domain.Entities
{
#nullable disable
    public sealed class Room
    {
        public string RoomNumber { get; set; }
        public int AdultCapacity { get; set; }
        public int ChildrenCapacity { get; set; }
        public string Thumbnail { get; set; }
        public int HotelId { get; set; }
        public Hotel Hotel { get; set; }
        public ICollection<RoomImage> Images { get; set; }
        public ICollection<BookingRoom> BookingRooms { get; set; }
    }
}
