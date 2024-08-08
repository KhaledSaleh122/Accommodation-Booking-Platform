namespace Domain.Entities
{
    public class BookingRoom : BaseEntity
    {
        public int BookingId { get; set; }
        public int HotelId { get; set; }
        public string RoomNumber { get; set; }

        public Room Room { get; set; }
        public Booking Booking { get; set; }
    }
}
