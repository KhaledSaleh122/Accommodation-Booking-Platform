using Application.CommandsAndQueries.BookingCQ.Commands.Confirm;
using Application.CommandsAndQueries.BookingCQ.Commands.Create;
using Application.CommandsAndQueries.BookingCQ.Commands.GenerateReport;
using Application.CommandsAndQueries.BookingCQ.Queries.GetUserbookingById;
using Application.CommandsAndQueries.BookingCQ.Queries.GetUserBookings;
using Application.Dtos.BookingDtos;
using Application.Exceptions;
using Asp.Versioning;
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
using System.Security.Claims;

namespace Presentation.Controllers
{
    /// <summary>
    /// Controller responsible for managing room bookings for users.
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/users/{userId}/bookings")]
    public class BookingController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<BookingController> _logger;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="BookingController"/> class.
        /// </summary>
        /// <param name="mediator">The mediator instance for handling commands.</param>
        /// <param name="logger">The logger instance for logging operations.</param>
        /// <param name="configuration">The configuration instance for accessing app settings.</param>
        /// <exception cref="ArgumentNullException">Thrown when the mediator or logger is null.</exception>
        public BookingController(IMediator mediator, ILogger<BookingController> logger, IConfiguration configuration)
        {
            _configuration = configuration;
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Creates a new room booking.
        /// </summary>
        /// <param name="command">The command containing booking details.</param>
        /// <returns>An <see cref="IActionResult"/> representing the result of the operation.</returns>
        /// <response code="201">Returns the created booking request.</response>
        /// <response code="400">If the command is null or validation fails.</response>
        /// <response code="401">If the user is not authenticated.</response>
        /// <response code="403">If the user is not authorized.</response>
        /// <response code="404">If the booking is not found.</response>
        /// <response code="409">If there is a conflict during the request.</response>
        /// <exception cref="CustomValidationException">Thrown when the command is null.</exception>
        [ApiVersion("1.0")]
        [HttpPost("/api/v{version:apiVersion}/users/bookings")]
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
            return CreatedAtAction(nameof(GetUserBooking), new { command.userId, bookingId = bookingRequest.Booking.Id }, bookingRequest);
        }

        /// <summary>
        /// Confirms a booking payment via Stripe.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> representing the result of the payment confirmation.</returns>
        [ApiVersion("1.0")]
        [HttpPost("/api/v{version:apiVersion}/users/bookings/payments")]
        public async Task<IActionResult> BookingConfirmation()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            var stripeEvent = EventUtility.ConstructEvent(json,
                    Request.Headers["Stripe-Signature"], _configuration.GetValue<string>("Stripe:EndpointSecret"));
            var command = new ConfirmBookingCommand() { Event = stripeEvent };
            await _mediator.Send(command);
            return Ok();
        }

        /// <summary>
        /// Retrieves a list of bookings made by a specific user.
        /// </summary>
        /// <param name="userId">The ID of the user whose bookings are to be retrieved.</param>
        /// <param name="startDate">Optional start date for filtering bookings.</param>
        /// <param name="endDate">Optional end date for filtering bookings.</param>
        /// <param name="page">The page number for pagination.</param>
        /// <param name="pageSize">The number of items per page for pagination.</param>
        /// <returns>An <see cref="IActionResult"/> containing the list of bookings.</returns>
        /// <response code="200">Returns the list of bookings.</response>
        /// <response code="401">If the user is not authenticated.</response>
        /// <response code="403">If the user is not authorized.</response>
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

        /// <summary>
        /// Retrieves the details of a specific booking made by a user.
        /// </summary>
        /// <param name="userId">The ID of the user whose booking is to be retrieved.</param>
        /// <param name="bookingId">The ID of the booking to be retrieved.</param>
        /// <returns>An <see cref="IActionResult"/> containing the booking details.</returns>
        /// <response code="200">Returns the booking details.</response>
        /// <response code="401">If the user is not authenticated.</response>
        /// <response code="403">If the user is not authorized.</response>
        /// <exception cref="NotFoundException">Thrown when the booking is not found.</exception>
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

        /// <summary>
        /// Generates a confirmation report for a specific booking.
        /// </summary>
        /// <param name="userId">The ID of the user whose booking report is to be generated.</param>
        /// <param name="bookingId">The ID of the booking for which the report is to be generated.</param>
        /// <returns>A PDF file containing the booking confirmation report.</returns>
        /// <response code="200">Returns the booking confirmation report as a PDF.</response>
        /// <response code="401">If the user is not authenticated.</response>
        /// <response code="403">If the user is not authorized.</response>
        /// <exception cref="NotFoundException">Thrown when the booking is not found.</exception>
        [HttpGet("{bookingId}/report")]
        [Authorize(Roles = "User,Admin")]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(BookingDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GenerateConfirmationReport(string userId, int bookingId)
        {
            if (bookingId <= 0) throw new NotFoundException("Booking not found!");
            var admin = User.FindFirst(claim => claim.Type == ClaimTypes.Role && claim.Value == "Admin");
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
            if (admin is null && userId != currentUserId)
            {
                return Forbid();
            }
            var query = new GenerateConfirmationReportQuery()
            {
                BookingId = bookingId,
                UserId = userId
            };

            var fileBytes = await _mediator.Send(query);

            return File(fileBytes, "application/pdf", "Invoice.pdf");
        }
    }
}
