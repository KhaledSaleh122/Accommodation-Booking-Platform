namespace Domain.Entities
{
    public sealed class Booking
    {
        #nullable disable
        public int Id { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }

        public decimal OriginalPrice { get; set; }
        public int DiscountPercentage { get; set; }
        public string RoomNumber { get; set; }
        public int HotelId { get; set; }
        public Room Room { get; set; }

        public string UserId { get; set; }
        public User User { get; set; }
    }
}
