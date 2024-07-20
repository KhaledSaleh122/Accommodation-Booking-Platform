using Application.CommandsAndQueries.CityCQ.Commands.Update;
using Application.CommandsAndQueries.HotelCQ.Commands.Create;
using Application.CommandsAndQueries.HotelCQ.Query.GetHotelById;
using Application.Dtos.HotelDtos;
using Application.Exceptions;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Presentation.Responses.NotFound;
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
    }
}
