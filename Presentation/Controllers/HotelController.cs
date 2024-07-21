using Application.CommandsAndQueries.CityCQ.Commands.Update;
using Application.CommandsAndQueries.HotelCQ.Commands.Create;
using Application.CommandsAndQueries.HotelCQ.Commands.Delete;
using Application.CommandsAndQueries.HotelCQ.Query.GetHotelById;
using Application.CommandsAndQueries.HotelCQ.Query.GetHotels;
using Application.Dtos.HotelDtos;
using Application.Exceptions;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Presentation.Responses.NotFound;
using Presentation.Responses.Pagination;
using Presentation.Responses.Validation;

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
            if (hotelId <= 0) throw new NotFoundException("Hotel not found!");
            var query = new GetHotelByIdQuery(hotelId);
            var hotelDto = await _mediator.Send(query);
            return Ok(hotelDto);
        }


        [HttpPatch("{hotelId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(NotFoundResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateHotel(int hotelId, UpdateHotelCommand? command)
        {
            if (command is null) return Ok();
            if (hotelId <= 0) throw new NotFoundException("Hotel not found!");
            command.hotelId = hotelId;
            await _mediator.Send(command);
            _logger.LogInformation("Hotel with id '{hotelId}' updated.", hotelId);
            return Ok();
        }

        [HttpPost]
        [ProducesResponseType(typeof(HotelMinDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationFailureResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateHotel([FromForm] CreateHotelCommand request)
        {
            if (request is null) throw new CustomValidationException("The request must include a body.");
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
        [ProducesResponseType(typeof(HotelMinDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(NotFoundResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<HotelMinDto>> DeleteHotel(int hotelId)
        {
            if(hotelId <= 0) throw new NotFoundException("Hotel not found!");
            var deleteHotelCommand = new DeleteHotelCommand(hotelId);
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
        [FromQuery] int[] aminites,
        decimal? maxPrice,
        decimal minPrice = 0,
        int page = 1,
        int pageSize = 10
    )
        {
            var query = new GetHotelsQuery
                (
                    page,
                    pageSize,
                    minPrice,
                    maxPrice,
                    city,
                    country,
                    hotelType,
                    hotelName,
                    owner,
                    aminites
                );
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
