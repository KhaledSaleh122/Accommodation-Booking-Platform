using Application.CommandsAndQueries.ReviewCQ.Commands.Create;
using Application.CommandsAndQueries.ReviewCQ.Commands.Delete;
using Application.Dtos.ReviewDtos;
using Application.Exceptions;
using Asp.Versioning;
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
    /// <summary>
    /// Controller responsible for managing reviews for hotels.
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/hotels/{hotelId}/reviews")]
    public class ReviewController : ControllerBase
    {
        private readonly ILogger<ReviewController> _logger;
        private readonly IMediator _mediator;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReviewController"/> class.
        /// </summary>
        /// <param name="logger">The logger instance for logging operations.</param>
        /// <param name="mediator">The mediator instance for handling commands.</param>
        /// <exception cref="ArgumentNullException">Thrown when the logger or mediator is null.</exception>
        public ReviewController(ILogger<ReviewController> logger, IMediator mediator)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        /// <summary>
        /// Creates a new review for a specific hotel.
        /// </summary>
        /// <param name="hotelId">The ID of the hotel to review.</param>
        /// <param name="command">The create command containing the review details.</param>
        /// <returns>An <see cref="IActionResult"/> representing the result of the operation.</returns>
        /// <response code="201">If the review is successfully created.</response>
        /// <response code="400">If the request body is invalid.</response>
        /// <response code="401">If the user is not authenticated.</response>
        /// <response code="403">If the user is not authorized.</response>
        /// <response code="404">If the hotel is not found.</response>
        /// <exception cref="NotFoundException">Thrown when the hotel is not found.</exception>
        /// <exception cref="CustomValidationException">Thrown when the request body is invalid.</exception>
        [HttpPost]
        [Authorize(Roles = "User")]
        [ProducesResponseType(typeof(ReviewDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(NotFoundResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ValidationFailureResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CreateHotelReview(int hotelId, CreateReviewCommand? command)
        {
            if (hotelId <= 0) throw new NotFoundException("Hotel not found!");
            if (command is null) throw new CustomValidationException("The request must include a valid body.");
            command.userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
            command.hotelId = hotelId;
            var reviewDto = await _mediator.Send(command);
            _logger.LogInformation(
               "New Review created: HotelId={command.hotelId}, UserId={command.userId}, Rate={command.Rating}",
               command.hotelId,
               command.userId,
               command.Rating);
            return CreatedAtRoute("GetHotelById", new { hotelId }, reviewDto);
        }

        /// <summary>
        /// Deletes a specific review for a hotel.
        /// </summary>
        /// <param name="hotelId">The ID of the hotel associated with the review.</param>
        /// <param name="userId">The ID of the user who created the review.</param>
        /// <returns>An <see cref="IActionResult"/> representing the result of the operation.</returns>
        /// <response code="200">If the review is successfully deleted.</response>
        /// <response code="401">If the user is not authenticated.</response>
        /// <response code="403">If the user is not authorized.</response>
        /// <response code="404">If the hotel or review is not found.</response>
        /// <exception cref="NotFoundException">Thrown when the hotel or review is not found.</exception>
        [HttpDelete("{userId}")]
        [Authorize(Roles = "User,Admin")]
        [ProducesResponseType(typeof(ReviewDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(NotFoundResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> DeleteHotelReview(int hotelId, string userId)
        {
            if (hotelId <= 0) throw new NotFoundException("Hotel not found!");
            var admin = User.FindFirst(claim => claim.Type == ClaimTypes.Role && claim.Value == "Admin");
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
            if (admin is null && userId != currentUserId)
            {
                return Forbid();
            }
            var command = new DeleteReviewCommand()
            {
                HotelId = hotelId,
                UserId = userId
            };
            var reviewDto = await _mediator.Send(command);
            _logger.LogInformation(
                "Review with HotelId '{command.HotelId}' and UserId '{command.UserId}' deleted.",
                command.HotelId,
                command.UserId);
            return Ok(reviewDto);
        }
    }
}
