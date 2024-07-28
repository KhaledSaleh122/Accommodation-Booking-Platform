namespace Application.Dtos.BookingDtos
{
    public class BookingRequestDto
    {
        public string ClientSecret { get; set; }
        public string PaymentIntentId { get; set; }
        public BookingDto Booking { get; set; }
    }
}
