using Application.Dtos.BookingDtos;
using MediatR;
using Microsoft.VisualBasic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Application.CommandsAndQueries.BookingCQ.Commands.Create
{
    public class CreateRoomBookingCommand : IRequest<BookingDto>
    {
        public int HotelId { get; set; }
        public string userId;
        public CreateBookingDto[] RoomBookings { get; set; }
    }
}
