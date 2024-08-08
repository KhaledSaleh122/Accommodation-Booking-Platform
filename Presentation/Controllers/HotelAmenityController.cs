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
    /// <summary>
    /// Controller responsible for managing hotel amenities.
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/hotels/{hotelId}/amenities")]
    public class HotelAmenityController : ControllerBase
    {
        private readonly ILogger<HotelAmenityController> _logger;
        private readonly IMediator _mediator;

        /// <summary>
        /// Initializes a new instance of the <see cref="HotelAmenityController"/> class.
        /// </summary>
        /// <param name="logger">The logger instance for logging operations.</param>
        /// <param name="mediator">The mediator instance for handling commands.</param>
        /// <exception cref="ArgumentNullException">Thrown when the logger or mediator is null.</exception>
        public HotelAmenityController(ILogger<HotelAmenityController> logger, IMediator mediator)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        /// <summary>
        /// Adds an amenity to a specific hotel.
        /// </summary>
        /// <param name="hotelId">The ID of the hotel to which the amenity will be added.</param>
        /// <param name="amenityId">The ID of the amenity to add.</param>
        /// <returns>An <see cref="ActionResult"/> representing the result of the operation.</returns>
        /// <response code="201">If the amenity is successfully added to the hotel.</response>
        /// <response code="401">If the user is not authenticated.</response>
        /// <response code="403">If the user is not authorized.</response>
        /// <response code="404">If the hotel or amenity is not found.</response>
        /// <exception cref="NotFoundException">Thrown when the hotel or amenity is not found.</exception>
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

        /// <summary>
        /// Removes an amenity from a specific hotel.
        /// </summary>
        /// <param name="hotelId">The ID of the hotel from which the amenity will be removed.</param>
        /// <param name="amenityId">The ID of the amenity to remove.</param>
        /// <returns>An <see cref="IActionResult"/> representing the result of the operation.</returns>
        /// <response code="200">If the amenity is successfully removed from the hotel.</response>
        /// <response code="401">If the user is not authenticated.</response>
        /// <response code="403">If the user is not authorized.</response>
        /// <response code="404">If the hotel or amenity is not found.</response>
        /// <exception cref="NotFoundException">Thrown when the hotel or amenity is not found.</exception>
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
            _logger.LogInformation("Amenity with id '{amenityId}' removed from hotel with id '{hotelId}'.", amenityId, hotelId);
            return Ok();
        }
    }
}
