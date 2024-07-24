using Application.CommandsAndQueries.RecentlyVisitedHotelCQ.Queries.GetRecentlyVisitedHotels;
using Application.Dtos.RecentlyVisitedHotelDto;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace Presentation.Controllers
{
    [ApiController]
    [Route("api/users/{userId}")]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private readonly IMediator _mediator;
        public UserController(ILogger<UserController> logger, IMediator mediator)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [HttpGet("recently-visited-hotels")]
        [Authorize(Roles = "User,Admin")]
        [ProducesResponseType(typeof(ICollection<RvhDto>),StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetRecentlyVisitedHotels(string userId) {
            var admin = User.FindFirst(claim => claim.Type == ClaimTypes.Role && claim.Value == "Admin");
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
            if (admin is null && userId != currentUserId)
            {
                return Forbid();
            }
            var command = new GetRvhCommand() { UserId = userId };
            var visitedHotels = await _mediator.Send(command);
            return Ok(visitedHotels);
        }


    }
}
