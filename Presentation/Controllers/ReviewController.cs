using Application.CommandsAndQueries.ReviewCQ.Commands;
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
        public async Task<IActionResult> CreateHotelReview(int hotelId, CreateReviewCommand? command)
        {
            if(hotelId <= 0) throw new NotFoundException("Hotel not found!");
            if (command is null) throw new CustomValidationException("The request must include a body.");
            command.userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
            command.hotelId = hotelId;
            var reviewDto = await _mediator.Send(command);
            return CreatedAtRoute("GetHotelById", new { hotelId }, reviewDto);
        }
    }
}
