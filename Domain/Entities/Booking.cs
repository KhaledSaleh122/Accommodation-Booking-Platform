namespace Domain.Entities
{
    public sealed class Booking
    {
        #nullable disable
        public int Id { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public string SpecialOfferId { get; set; }
        public decimal OriginalTotalPrice { get; set; }
        public decimal DiscountedTotalPrice { get; set; }
        public ICollection<BookingRoom> BookingRooms { get; set; }
        public SpecialOffer SpecialOffer { get; set; }
        public string UserId { get; set; }
        public User User { get; set; }
    }
}
