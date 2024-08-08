using Application.CommandsAndQueries.HotelAmenityCQ.Commands.Create;
using Application.CommandsAndQueries.HotelAmenityCQ.Commands.Delete;
using Application.Exceptions;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Presentation.Responses.NotFound;
namespace Presentation.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/hotels/{hotelId}/amenities")]
    public class HotelAmenityController : ControllerBase
    {
        private readonly ILogger<HotelAmenityController> _logger;
        private readonly IMediator _mediator;
        public HotelAmenityController(ILogger<HotelAmenityController> logger, IMediator mediator)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [HttpPost("{amenityId}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(NotFoundResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult> AddAmenityToHotel(int hotelId, int amenityId)
        {
            if (hotelId <= 0) throw new NotFoundException("Hotel not found!");
            if (amenityId <= 0) throw new NotFoundException("Amenity not found!");
            var addAmenityCommand = new AddAmenityToHotelCommand(hotelId, amenityId);
            await _mediator.Send(addAmenityCommand);
            _logger.LogInformation("Amenity with id '{amenityId}' added to hotel with id '{hotelId}'.", amenityId, hotelId);

            return CreatedAtRoute("GetHotelById", new { hotelId }, null);
        }

        [HttpDelete("{amenityId}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(NotFoundResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> RemoveAmenityFromHotel(int hotelId, int amenityId)
        {
            if (hotelId <= 0) throw new NotFoundException("Hotel not found!");
            if (amenityId <= 0) throw new NotFoundException("Amenity not found!");
            var addAmenityCommand = new RemoveAmenityFromHotelCommand(hotelId, amenityId);
            await _mediator.Send(addAmenityCommand);
            _logger.LogInformation(
                "Amenity with id '{amenityId}' removed from hotel with id '{hotelId}'.", 
                amenityId,
                hotelId
            );
            return Ok();
        }
    }
}
