using Application.CommandsAndQueries.CityCQ.Commands.Update;
using Application.CommandsAndQueries.HotelCQ.Commands.Create;
using Application.CommandsAndQueries.HotelCQ.Commands.Delete;
using Application.CommandsAndQueries.HotelCQ.Query.GetHotelById;
using Application.CommandsAndQueries.HotelCQ.Query.GetHotels;
using Application.Dtos.HotelDtos;
using Application.Exceptions;
using Asp.Versioning;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Presentation.Responses.NotFound;
using Presentation.Responses.Pagination;
using Presentation.Responses.Validation;
using System.Security.Claims;

namespace Presentation.Controllers
{
    /// <summary>
    /// Controller responsible for managing hotels.
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/hotels")]
    public class HotelController : ControllerBase
    {
        private readonly ILogger<HotelController> _logger;
        private readonly IMediator _mediator;

        /// <summary>
        /// Initializes a new instance of the <see cref="HotelController"/> class.
        /// </summary>
        /// <param name="logger">The logger instance for logging operations.</param>
        /// <param name="mediator">The mediator instance for handling commands.</param>
        /// <exception cref="ArgumentNullException">Thrown when the logger or mediator is null.</exception>
        public HotelController(ILogger<HotelController> logger, IMediator mediator)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        /// <summary>
        /// Retrieves the available hotel types.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> containing a dictionary of hotel types.</returns>
        /// <response code="200">Returns the available hotel types.</response>
        [HttpGet("hotelTypes")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetHotelTypes()
        {
            return Ok(
                        Enum.GetValues(typeof(HotelType))
                            .Cast<HotelType>()
                            .ToDictionary(k => k.ToString(), v => (int)v)
                      );
        }

        /// <summary>
        /// Retrieves a specific hotel by its ID.
        /// </summary>
        /// <param name="hotelId">The ID of the hotel to retrieve.</param>
        /// <returns>An <see cref="ActionResult"/> containing the hotel details.</returns>
        /// <response code="200">Returns the hotel details.</response>
        /// <response code="404">If the hotel is not found.</response>
        /// <exception cref="NotFoundException">Thrown when the hotel is not found.</exception>
        [HttpGet("{hotelId}", Name = "GetHotelById")]
        [ProducesResponseType(typeof(HotelFullDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(NotFoundResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<HotelFullDto>> GetHotel(int hotelId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
            if (hotelId <= 0) throw new NotFoundException("Hotel not found!");
            var query = new GetHotelByIdQuery(hotelId) { UserId = userId };
            var hotelDto = await _mediator.Send(query);
            return Ok(hotelDto);
        }

        /// <summary>
        /// Updates a specific hotel.
        /// </summary>
        /// <param name="hotelId">The ID of the hotel to update.</param>
        /// <param name="command">The update command containing the new hotel details.</param>
        /// <returns>An <see cref="IActionResult"/> representing the result of the operation.</returns>
        /// <response code="200">If the hotel is successfully updated.</response>
        /// <response code="401">If the user is not authenticated.</response>
        /// <response code="403">If the user is not authorized.</response>
        /// <response code="404">If the hotel is not found.</response>
        /// <exception cref="NotFoundException">Thrown when the hotel is not found.</exception>
        [HttpPatch("{hotelId}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(HotelMinDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(NotFoundResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> UpdateHotel(int hotelId, UpdateHotelCommand? command)
        {
            if (command is null) return Ok();
            if (hotelId <= 0) throw new NotFoundException("Hotel not found!");
            command.hotelId = hotelId;
            var hotel = await _mediator.Send(command);
            _logger.LogInformation("Hotel with id '{hotelId}' updated.", hotelId);
            return Ok(hotel);
        }

        /// <summary>
        /// Creates a new hotel.
        /// </summary>
        /// <param name="request">The create command containing the hotel details.</param>
        /// <returns>An <see cref="IActionResult"/> representing the result of the operation.</returns>
        /// <response code="201">If the hotel is successfully created.</response>
        /// <response code="400">If the request body is invalid.</response>
        /// <response code="401">If the user is not authenticated.</response>
        /// <response code="403">If the user is not authorized.</response>
        /// <exception cref="CustomValidationException">Thrown when the request body is invalid.</exception>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(HotelMinDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationFailureResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CreateHotel([FromForm] CreateHotelCommand request)
        {
            if (request is null) throw new CustomValidationException("The request must include a valid body.");
            var hotelDto = await _mediator.Send(request);
            _logger.LogInformation(
                "New hotel created: Id={hotelDto.Id}, Name={hotelDto.Name}",
                hotelDto.Id,
                hotelDto.Name);
            return CreatedAtAction(
                nameof(GetHotel),
                new { hotelId = hotelDto.Id },
                hotelDto
            );
        }

        /// <summary>
        /// Deletes a specific hotel by its ID.
        /// </summary>
        /// <param name="hotelId">The ID of the hotel to delete.</param>
        /// <returns>An <see cref="ActionResult"/> representing the result of the operation.</returns>
        /// <response code="200">If the hotel is successfully deleted.</response>
        /// <response code="401">If the user is not authenticated.</response>
        /// <response code="403">If the user is not authorized.</response>
        /// <response code="404">If the hotel is not found.</response>
        /// <exception cref="NotFoundException">Thrown when the hotel is not found.</exception>
        [HttpDelete("{hotelId}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(HotelMinDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(NotFoundResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<HotelMinDto>> DeleteHotel(int hotelId)
        {
            if (hotelId <= 0) throw new NotFoundException("Hotel not found!");
            var deleteHotelCommand = new DeleteHotelCommand() { Id = hotelId };
            var deletedHotel = await _mediator.Send(deleteHotelCommand);
            _logger.LogInformation("Hotel with id '{deletedHotel.Id}' deleted.", deletedHotel.Id);
            return Ok(deletedHotel);
        }

        /// <summary>
        /// Retrieves a list of hotels based on the specified search criteria.
        /// </summary>
        /// <param name="city">The city where the hotel is located.</param>
        /// <param name="country">The country where the hotel is located.</param>
        /// <param name="hotelType">The types of hotels to retrieve.</param>
        /// <param name="hotelName">The name of the hotel to search for.</param>
        /// <param name="owner">The owner of the hotel.</param>
        /// <param name="amenities">The amenities available in the hotel.</param>
        /// <param name="maxPrice">The maximum price of the hotel.</param>
        /// <param name="checkInDate">The check-in date for the hotel.</param>
        /// <param name="checkoutDate">The checkout date for the hotel.</param>
        /// <param name="numberOfAdults">The number of adults staying in the hotel.</param>
        /// <param name="numberOfChildren">The number of children staying in the hotel.</param>
        /// <param name="minPrice">The minimum price of the hotel.</param>
        /// <param name="page">The page number of the results to retrieve.</param>
        /// <param name="pageSize">The number of results per page.</param>
        /// <returns>An <see cref="ActionResult"/> containing the list of hotels with pagination.</returns>
        /// <response code="200">Returns the list of hotels with pagination.</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<HotelDto>>> GetHotels
        (
            string? city,
            string? country,
            [FromQuery] HotelType[] hotelType,
            string? hotelName,
            string? owner,
            [FromQuery] int[] amenities,
            decimal? maxPrice,
            DateOnly? checkInDate = null,
            DateOnly? checkoutDate = null,
            int numberOfAdults = 2,
            int numberOfChildren = 0,
            decimal minPrice = 0,
            int page = 1,
            int pageSize = 10
        )
        {
            var query = new GetHotelsQuery(page, pageSize)
            {
                City = city,
                Country = country,
                HotelType = hotelType,
                HotelName = hotelName,
                Owner = owner,
                Amenities = amenities,
                MaxPrice = maxPrice,
                MinPrice = minPrice,
                CheckIn = checkInDate,
                CheckOut = checkoutDate,
                Adult = numberOfAdults,
                Children = numberOfChildren
            };
            var (hotels, totalRecords, thePage, thePageSize) = await _mediator.Send(query);
            var response = new ResultWithPaginationResponse<IEnumerable<HotelDto>>()
            {
                TotalRecords = totalRecords,
                Page = thePage,
                PageSize = thePageSize,
                Results = hotels
            };
            return Ok(response);
        }
    }
}
