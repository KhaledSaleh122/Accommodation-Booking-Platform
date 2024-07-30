using MediatR;
using Stripe;

namespace Application.CommandsAndQueries.BookingCQ.Commands.Confirm
{
    public class ConfirmBookingCommand : IRequest
    {
        public Event Event { get; set; }
    }
}
