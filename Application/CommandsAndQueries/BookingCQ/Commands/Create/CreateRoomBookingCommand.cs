using Application.Dtos.BookingDtos;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Application.CommandsAndQueries.BookingCQ.Commands.Create
{
    public class CreateRoomBookingCommand : IRequest<BookingRequestDto>
    {
        public string userId;
        [Required]
        public int HotelId { get; set; }
        public string? SpecialOfferId { get; set; }
        public HashSet<string> RoomsNumbers { get; set; }
        [Required]
        public DateOnly StartDate { get; set; }
        [Required]
        public DateOnly EndDate { get; set; }
    }
}
