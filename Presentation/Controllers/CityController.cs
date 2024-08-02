using Application.CommandsAndQueries.CityCQ.Commands.Create;
using Application.CommandsAndQueries.CityCQ.Commands.Delete;
using Application.CommandsAndQueries.CityCQ.Commands.Update;
using Application.CommandsAndQueries.CityCQ.Queries.TopVisitedCities;
using Application.CommandsAndQueries.CityCQ.Query.GetCities;
using Application.CommandsAndQueries.CityCQ.Query.GetCityById;
using Application.Dtos.CityDtos;
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
    [Route("api/cities")]
    public class CityController : ControllerBase
    {
        private readonly ILogger<CityController> _logger;
        private readonly IMediator _mediator;
        public CityController(ILogger<CityController> logger, IMediator mediator)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }


        [HttpGet("{cityId}")]
        [ProducesResponseType(typeof(CityDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(NotFoundResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CityDto>> GetCity(int cityId)
        {
            if (cityId <= 0 ) throw new NotFoundException("City not found!");
            var query = new GetCityByIdQuery() { CityId = cityId };
            var cityDto = await _mediator.Send(query);
            return Ok(cityDto);
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<CityDto>>> GetCities(
                string? city,
                string? country,
                int page = 1,
                int pageSize = 10
            )
        {
            var query = new GetCitiesQuery(page,pageSize)
            {
                Country = country,
                City = city
            };
            var (cities, totalRecords, thePage, thePageSize) = await _mediator.Send(query);
            var response = new ResultWithPaginationResponse<IEnumerable<CityDto>>()
            {
                TotalRecords = totalRecords,
                Page = thePage,
                PageSize = thePageSize,
                Results = cities
            };
            return Ok(response);
        }


        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(CityDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationFailureResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateCity([FromForm]CreateCityCommand? request)
        {
            if (request is null) throw new CustomValidationException("The request must include a valid body.");
            var cityDto = await _mediator.Send(request);
            _logger.LogInformation("New city created: Id={cityDto.Id}, Name={cityDto.Name}", cityDto.Id, cityDto.Name);
            return CreatedAtAction(
                nameof(GetCity),
                new { cityId = cityDto.Id },
                cityDto
            );
        }

        [HttpPut("{cityId}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(CityDto),StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(NotFoundResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> UpdateCity(int cityId, [FromForm]UpdateCityCommand? command)
        {
            if (cityId <= 0) throw new NotFoundException("City not found!");
            if (command is null) return Ok();
            command.id = cityId;
            var city = await _mediator.Send(command);
            _logger.LogInformation("City with id '{cityId}' updated.", cityId);
            return Ok(city);
        }

        [HttpDelete("{cityId}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(CityDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(NotFoundResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<CityDto>> DeleteCity(int cityId)
        {
            if (cityId <= 0) throw new NotFoundException("City not found!");
            var deleteCityCommand = new DeleteCityCommand() { Id = cityId };
            var deletedCity = await _mediator.Send(deleteCityCommand);
            _logger.LogInformation("City with id '{deletedCity.Id}' deleted.", deletedCity.Id);
            return Ok(deletedCity);
        }

        [HttpGet("top-visited-cities")]
        [ProducesResponseType(typeof(CityTopDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<CityTopDto>>> GetTopVisitedCities() {
            var command = new GetTopVisitedCitiesQuery();
            var cities = await _mediator.Send(command);
            return Ok(cities);
        }

    }
}
