using Application.CommandsAndQueries.AmenityCQ.Commands.Create;
using Application.CommandsAndQueries.AmenityCQ.Query.GetAmenityById;
using Application.Dtos.AmenityDtos;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Presentation.Responses.NotFound;
using Presentation.Responses.Validation;
using System.ComponentModel.DataAnnotations;

namespace Presentation.Controllers
{

    [ApiController]
    [Route("api/amenities")]
    public class AmenityController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<AmenityController> _logger;
        public AmenityController(IMediator mediator, ILogger<AmenityController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpPost]
        [ProducesResponseType(typeof(AmenityDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationFailureResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateAmenity([Required] CreateAmenityCommand request)
        {
            var amenityDto = await _mediator.Send(request);
            _logger.LogInformation
            (
                "New amenity created: Id={amenityDto.Id}, Name={amenityDto.Name}," +
                " Description={amenityDto.Description}",
                amenityDto.Id,
                amenityDto.Name,
                amenityDto.Description
            );
            return CreatedAtAction(
                nameof(GetAmenity),
                new { amenityId = amenityDto.Id },
                amenityDto
            );
        }

        [HttpGet("{amenityId}")]
        [ProducesResponseType(typeof(AmenityDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(NotFoundResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AmenityDto>> GetAmenity(int amenityId)
        {
            var query = new GetAmenityByIdQuery(amenityId);
            var amenityDto = await _mediator.Send(query);
            if (amenityDto is null) return NotFound();
            return Ok(amenityDto);
        }


    }
}
