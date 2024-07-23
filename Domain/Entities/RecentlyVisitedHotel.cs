namespace Domain.Entities
{
    public sealed class RecentlyVisitedHotel
    {
        public int HotelId { get; set; }
        public Hotel Hotel { get; set; }
        public string UserId { get; set; }
        public User User { get; set; }

        public DateTime VisitedDate { get; set; }
    }
}
