using Application.CommandsAndQueries.AmenityCQ.Query.GetAmenityById;
using Application.Dtos.AmenityDtos;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Presentation.Responses.NotFound;

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
