namespace Application.Dtos.BookingDtos
{
    public class BookingRoomDto
    {
        public int Id { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
    }
}
