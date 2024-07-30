using Application.CommandsAndQueries.BookingCQ.Commands.Create;
using Application.CommandsAndQueries.BookingCQ.Queries.GetUserbookingById;
using Application.CommandsAndQueries.BookingCQ.Queries.GetUserBookings;
using Application.Dtos.BookingDtos;
using Application.Exceptions;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Presentation.Responses.NotFound;
using Presentation.Responses.Pagination;
using Presentation.Responses.ServerErrors;
using Presentation.Responses.Validation;
using Stripe;
using System.Net.Mail;
using System.Net;
using System.Security.Claims;
using Application.CommandsAndQueries.BookingCQ.Commands.Confirm;
namespace Presentation.Controllers
{
    [ApiController]
    [Route("api/users/{userId}/bookings")]
    public class BookingController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<BookingController> _logger;
        private readonly IConfiguration _configuration;
        public BookingController(IMediator mediator, ILogger<BookingController> logger, IConfiguration configuration)
        {
            _configuration = configuration;
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        [HttpPost("/api/users/bookings")]
        [Authorize(Roles = "User")]
        [ProducesResponseType(typeof(ServerErrorResponse), StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(BookingRequestDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationFailureResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(NotFoundResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreateRoomBooking([FromBody] CreateRoomBookingCommand? command)
        {
            if (command is null) throw new CustomValidationException("The request must include a valid body.");
            command.userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
            var bookingRequest = await _mediator.Send(command);
            _logger.LogInformation(
                "A new request with ID '{BookingId}' has been created to book a room.",
                bookingRequest.Booking.Id);
            return Ok(bookingRequest);
        }
        [HttpPost("/api/users/bookings/payments")]
        public async Task<IActionResult> BookingConfirmation() {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            var stripeEvent = EventUtility.ConstructEvent(json,
                    Request.Headers["Stripe-Signature"], _configuration.GetValue<string>("Stripe:EndpointSecret"));
            var command = new ConfirmBookingCommand() { Event = stripeEvent };
            await _mediator.Send(command);
            return Ok();
        }

        [HttpGet]
        [Authorize(Roles = "User,Admin")]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ResultWithPaginationResponse<IEnumerable<BookingDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUserBookings(
            string userId,
            DateOnly? startDate = null,
            DateOnly? endDate = null,
            int page = 1,
            int pageSize = 10
            )
        {
            var admin = User.FindFirst(claim => claim.Type == ClaimTypes.Role && claim.Value == "Admin");
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
            if (admin is null && userId != currentUserId)
            {
                return Forbid();
            }
            var query = new GetUserBookingsQuery(page, pageSize)
            {
                EndDate = endDate,
                StartDate = startDate,
                UserId = userId
            };
            var (bookings, totalRecords, thePage, thePageSize) = await _mediator.Send(query);
            var response = new ResultWithPaginationResponse<IEnumerable<BookingDto>>()
            {
                TotalRecords = totalRecords,
                Page = thePage,
                PageSize = thePageSize,
                Results = bookings
            };
            return Ok(response);
        }

        [HttpGet("{bookingId}")]
        [Authorize(Roles = "User,Admin")]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(BookingDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUserBooking(string userId, int bookingId)
        {
            if (bookingId <= 0) throw new NotFoundException("Booking not found!");
            var admin = User.FindFirst(claim => claim.Type == ClaimTypes.Role && claim.Value == "Admin");
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
            if (admin is null && userId != currentUserId)
            {
                return Forbid();
            }
            var query = new GetUserBookingByIdQuery()
            {
                BookingId = bookingId,
                UserId = userId
            };
            var booking = await _mediator.Send(query);
            return Ok(booking);
        }


    }
}
