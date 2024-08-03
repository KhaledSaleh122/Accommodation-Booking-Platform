using Application.CommandsAndQueries.CityCQ.Commands.Update;
using Application.CommandsAndQueries.HotelCQ.Commands.Create;
using Application.CommandsAndQueries.HotelCQ.Commands.Delete;
using Application.CommandsAndQueries.HotelCQ.Query.GetHotelById;
using Application.CommandsAndQueries.HotelCQ.Query.GetHotels;
using Application.Dtos.HotelDtos;
using Application.Exceptions;
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
    [ApiController]
    [Route("api/hotels")]
    public class HotelController : ControllerBase
    {
        private readonly ILogger<HotelController> _logger;
        private readonly IMediator _mediator;
        public HotelController(ILogger<HotelController> logger, IMediator mediator)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }


        [HttpGet("hotelTypes")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetHotelTyps()
        {
            return Ok(
                        Enum.GetValues(typeof(HotelType))
                            .Cast<HotelType>()
                            .ToDictionary(k => k.ToString(), v => (int)v)
                      );
        }

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


        [HttpPatch("{hotelId}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(HotelMinDto),StatusCodes.Status200OK)]
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
            int numberOfchildrens = 0,
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
                Children = numberOfchildrens
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
