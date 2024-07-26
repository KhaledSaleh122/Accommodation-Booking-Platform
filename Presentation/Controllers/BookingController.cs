using Application.CommandsAndQueries.BookingCQ.Commands.Create;
using Application.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Presentation.Responses.NotFound;
using Presentation.Responses.Validation;
using System.Security.Claims;
namespace Presentation.Controllers
{
    [ApiController]
    [Route("api/bookings")]
    public class BookingController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<BookingController> _logger;
        public BookingController(IMediator mediator, ILogger<BookingController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        [HttpPost]
        [Authorize(Roles = "User")]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ValidationFailureResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(NotFoundResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreateRoomBooking([FromBody] CreateRoomBookingCommand? command)
        {
            if (command is null) throw new CustomValidationException("The request must include a valid body.");
            command.userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
            //command.hotelId = hotelId;
            //command.roomNumber = roomNumber;
            var bookingDto = await _mediator.Send(command);
            return Ok(bookingDto);
        }

    }
}
