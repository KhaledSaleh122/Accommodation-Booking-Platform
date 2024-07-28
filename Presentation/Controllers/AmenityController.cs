using Application.CommandsAndQueries.AmenityCQ.Commands.Create;
using Application.CommandsAndQueries.AmenityCQ.Commands.Delete;
using Application.CommandsAndQueries.AmenityCQ.Commands.Update;
using Application.CommandsAndQueries.AmenityCQ.Query.GetAmenities;
using Application.CommandsAndQueries.AmenityCQ.Query.GetAmenityById;
using Application.Dtos.AmenityDtos;
using Application.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Presentation.Responses.NotFound;
using Presentation.Responses.Pagination;
using Presentation.Responses.Validation;

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
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(AmenityDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationFailureResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateAmenity(CreateAmenityCommand request)
        {
            if (request is null) throw new CustomValidationException("The request must include a valid body.");
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
            if (amenityId <= 0) throw new NotFoundException("Amenity not found");
            var query = new GetAmenityByIdQuery() { 
             AmenityId = amenityId
            };
            var amenityDto = await _mediator.Send(query);
            return Ok(amenityDto);
        }

        [HttpGet]
        [ProducesResponseType(typeof(ResultWithPaginationResponse<IEnumerable<AmenityDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ResultWithPaginationResponse<IEnumerable<AmenityDto>>>>
            GetAmenities(int page = 1, int pageSize = 10)
        {
            var query = new GetAmenitiesQuery(page, pageSize);
            var (amenities, totalRecords, thePage, thePageSize) = await _mediator.Send(query);
            var response = new ResultWithPaginationResponse<IEnumerable<AmenityDto>>()
            {
                TotalRecords = totalRecords,
                Page = thePage,
                PageSize = thePageSize,
                Results = amenities
            };
            return Ok(response);
        }

        [HttpPut("{amenityId}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(AmenityDto),StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(NotFoundResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateAmenity(int amenityId, UpdateAmenityCommand? command)
        {
            if (command is null) return Ok();
            if (amenityId <= 0) throw new NotFoundException("Amenity not found");
            command.id = amenityId;
            var updatedAmenity = await _mediator.Send(command);
            _logger.LogInformation("Amenity with id '{AmenityId}' updated.", amenityId);
            return Ok(updatedAmenity);
        }


        [HttpDelete("{amenityId}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(AmenityDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(NotFoundResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AmenityDto>> DeleteAmenity(int amenityId)
        {
            if (amenityId <= 0) throw new NotFoundException("Amenity not found");
            var deleteAmenityCommand = new DeleteAmenityCommand() { Id = amenityId };
            var deletedAmenity = await _mediator.Send(deleteAmenityCommand);
            _logger.LogInformation("Amenity with id '{deletedAmenity.Id}' deleted.", deletedAmenity.Id);
            return Ok(deletedAmenity);
        }

    }
}
