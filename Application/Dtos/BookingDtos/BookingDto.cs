namespace Application.Dtos.BookingDtos
{
    public class BookingDto
    {
        public int Id { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public string SpecialOfferId { get; set; }
        public decimal OriginalTotalPrice { get; set; }
        public decimal DiscountedTotalPrice { get; set; }
        public int HotelId { get; set; }
        public ICollection<string> Rooms { get; set; }

    }
}
