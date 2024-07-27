using Application.Dtos.BookingDtos;
using MediatR;
using Microsoft.VisualBasic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Application.CommandsAndQueries.BookingCQ.Commands.Create
{
    public class CreateRoomBookingCommand : IRequest<BookingDto>
    {
        public string userId;
        [Required]
        public int HotelId { get; set; }
        public string? SpecialOfferId { get; set; }
        public HashSet <string> RoomsNumbers { get; set; }
        [Required]
        public DateOnly StartDate { get; set; }
        [Required]
        public DateOnly EndDate { get; set; }
        public CreditCardInformationDto CreditCard { get; set; }
    }
}
