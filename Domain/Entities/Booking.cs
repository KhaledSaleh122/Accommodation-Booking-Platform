namespace Domain.Entities
{
    public sealed class Booking
    {
        #nullable disable
        public int Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public string RoomNumber { get; set; }
        public int HotelId { get; set; }
        public Room Room { get; set; }

        public string UserId { get; set; }
        public User User { get; set; }
    }
}
