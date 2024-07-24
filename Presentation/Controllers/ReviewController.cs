using Application.CommandsAndQueries.ReviewCQ.Commands.Create;
using Application.CommandsAndQueries.ReviewCQ.Commands.Delete;
using Application.Dtos.HotelDtos;
using Application.Dtos.ReviewDtos;
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
    [Route("api/hotels/{hotelId}/reviews")]
    public class ReviewController : ControllerBase
    {
        private readonly ILogger<ReviewController> _logger;
        private readonly IMediator _mediator;
        public ReviewController(ILogger<ReviewController> logger, IMediator mediator)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [HttpPost]
        [Authorize(Roles = "User")]
        [ProducesResponseType(typeof(ReviewDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(NotFoundResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ValidationFailureResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CreateHotelReview(int hotelId, CreateReviewCommand? command)
        {
            if(hotelId <= 0) throw new NotFoundException("Hotel not found!");
            if (command is null) throw new CustomValidationException("The request must include a body.");
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

        [HttpDelete("{userId}")]
        [Authorize(Roles = "User,Admin")]
        [ProducesResponseType(typeof(ReviewDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(NotFoundResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> DeleteHotelReview(int hotelId,string userId) {
            if (hotelId <= 0) throw new NotFoundException("Hotel not found!");
            var admin = User.FindFirst(claim => claim.Type == ClaimTypes.Role && claim.Value == "Admin");
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
            if (admin is null && userId != currentUserId) {
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
