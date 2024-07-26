using System.ComponentModel.DataAnnotations;

namespace Application.Dtos.BookingDtos
{
    public class CreateBookingDto
    {
        [Required]
        public int HotelId { get; set; }
        [Required]
        public string RoomNumber { get; set; }
        [Required]
        public DateOnly StartDate { get; set; }
        [Required]
        public DateOnly EndDate { get; set; }
    }
}
