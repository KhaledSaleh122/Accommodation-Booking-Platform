using Application.Dtos.BookingDtos;
using MediatR;
using Microsoft.VisualBasic;

namespace Application.CommandsAndQueries.BookingCQ.Commands.Create
{
    public class CreateRoomBookingCommand : IRequest<BookingDto>
    {
        public int hotelId;
        public string roomNumber;
        public string userId;
        public DateOnly StartDate { get; set; }
        public int DaysToStay { get; set; }
    }
}
