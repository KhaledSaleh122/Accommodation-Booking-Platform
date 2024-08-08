using Application.CommandsAndQueries.RecentlyVisitedHotelCQ.Queries.GetRecentlyVisitedHotels;
using Application.Dtos.RecentlyVisitedHotelDto;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace Presentation.Controllers
{
    /// <summary>
    /// Controller responsible for managing user-related operations, including retrieving recently visited hotels.
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/users/{userId}")]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private readonly IMediator _mediator;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserController"/> class.
        /// </summary>
        /// <param name="logger">The logger instance for logging operations.</param>
        /// <param name="mediator">The mediator instance for handling queries.</param>
        /// <exception cref="ArgumentNullException">Thrown when the logger or mediator is null.</exception>
        public UserController(ILogger<UserController> logger, IMediator mediator)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        /// <summary>
        /// Retrieves a list of recently visited hotels for a specified user.
        /// </summary>
        /// <param name="userId">The ID of the user whose recently visited hotels are to be retrieved.</param>
        /// <returns>An <see cref="IActionResult"/> containing a collection of recently visited hotels.</returns>
        /// <response code="200">If the recently visited hotels are successfully retrieved.</response>
        /// <response code="401">If the user is not authenticated.</response>
        /// <response code="403">If the user is not authorized to access the information.</response>
        /// <exception cref="ForbidResult">Thrown when the current user is not authorized to access the data of another user.</exception>
        [HttpGet("recently-visited-hotels")]
        [Authorize(Roles = "User,Admin")]
        [ProducesResponseType(typeof(ICollection<RvhDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetRecentlyVisitedHotels(string userId)
        {
            var admin = User.FindFirst(claim => claim.Type == ClaimTypes.Role && claim.Value == "Admin");
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
            if (admin is null && userId != currentUserId)
            {
                return Forbid();
            }
            var command = new GetRvhQuery() { UserId = userId };
            var visitedHotels = await _mediator.Send(command);
            return Ok(visitedHotels);
        }
    }
}
