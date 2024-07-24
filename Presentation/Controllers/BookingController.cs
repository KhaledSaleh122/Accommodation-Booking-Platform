using Application.CommandsAndQueries.BookingCQ.Commands.Create;
using Application.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
namespace Presentation.Controllers
{
    [ApiController]
    [Route("api")]
    public class BookingController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<BookingController> _logger;
        public BookingController(IMediator mediator, ILogger<BookingController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        [HttpPost("hotels/{hotelId}/rooms/{roomNumber}/bookings")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> CreateRoomBooking(int hotelId, string roomNumber,[FromBody] CreateRoomBookingCommand? command)
        {
            if (hotelId <= 0) throw new NotFoundException("Hotel not found!");
            if (command is null) throw new CustomValidationException("The body for this request required");
            command.userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
            command.hotelId = hotelId;
            command.roomNumber = roomNumber;
            var bookingDto = await _mediator.Send(command);
            return Ok(bookingDto);
        }

    }
}
