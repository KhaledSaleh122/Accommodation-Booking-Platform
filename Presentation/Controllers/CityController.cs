using Application.CommandsAndQueries.CityCQ.Commands.Create;
using Application.CommandsAndQueries.CityCQ.Commands.Delete;
using Application.CommandsAndQueries.CityCQ.Commands.Update;
using Application.CommandsAndQueries.CityCQ.Queries.TopVisitedCities;
using Application.CommandsAndQueries.CityCQ.Query.GetCities;
using Application.CommandsAndQueries.CityCQ.Query.GetCityById;
using Application.Dtos.CityDtos;
using Application.Exceptions;
using Asp.Versioning;
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
    /// <summary>
    /// Controller responsible for managing cities.
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/cities")]
    public class CityController : ControllerBase
    {
        private readonly ILogger<CityController> _logger;
        private readonly IMediator _mediator;

        /// <summary>
        /// Initializes a new instance of the <see cref="CityController"/> class.
        /// </summary>
        /// <param name="logger">The logger instance for logging operations.</param>
        /// <param name="mediator">The mediator instance for handling commands.</param>
        /// <exception cref="ArgumentNullException">Thrown when the logger or mediator is null.</exception>
        public CityController(ILogger<CityController> logger, IMediator mediator)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        /// <summary>
        /// Retrieves details of a specific city by its ID.
        /// </summary>
        /// <param name="cityId">The ID of the city to retrieve.</param>
        /// <returns>An <see cref="ActionResult{CityDto}"/> containing the city details.</returns>
        /// <response code="200">Returns the city details.</response>
        /// <response code="404">If the city is not found.</response>
        /// <exception cref="NotFoundException">Thrown when the city is not found.</exception>
        [HttpGet("{cityId}")]
        [ProducesResponseType(typeof(CityDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(NotFoundResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CityDto>> GetCity(int cityId)
        {
            if (cityId <= 0) throw new NotFoundException("City not found!");
            var query = new GetCityByIdQuery() { CityId = cityId };
            var cityDto = await _mediator.Send(query);
            return Ok(cityDto);
        }

        /// <summary>
        /// Retrieves a list of cities, optionally filtered by name and country.
        /// </summary>
        /// <param name="city">The name of the city to filter by.</param>
        /// <param name="country">The name of the country to filter by.</param>
        /// <param name="page">The page number for pagination.</param>
        /// <param name="pageSize">The number of items per page for pagination.</param>
        /// <returns>An <see cref="ActionResult{IEnumerable{CityDto}}"/> containing the list of cities.</returns>
        /// <response code="200">Returns the list of cities.</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<CityDto>>> GetCities(
                string? city,
                string? country,
                int page = 1,
                int pageSize = 10
            )
        {
            var query = new GetCitiesQuery(page, pageSize)
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

        /// <summary>
        /// Creates a new city.
        /// </summary>
        /// <param name="request">The request containing city creation details.</param>
        /// <returns>An <see cref="IActionResult"/> representing the result of the operation.</returns>
        /// <response code="201">Returns the created city.</response>
        /// <response code="400">If the request is null or validation fails.</response>
        /// <response code="401">If the user is not authenticated.</response>
        /// <response code="403">If the user is not authorized.</response>
        /// <exception cref="CustomValidationException">Thrown when the request is null.</exception>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(CityDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationFailureResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateCity([FromForm] CreateCityCommand? request)
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

        /// <summary>
        /// Updates an existing city.
        /// </summary>
        /// <param name="cityId">The ID of the city to update.</param>
        /// <param name="command">The command containing the update details.</param>
        /// <returns>An <see cref="IActionResult"/> representing the result of the operation.</returns>
        /// <response code="200">Returns the updated city.</response>
        /// <response code="401">If the user is not authenticated.</response>
        /// <response code="403">If the user is not authorized.</response>
        /// <response code="404">If the city is not found.</response>
        /// <exception cref="NotFoundException">Thrown when the city is not found.</exception>
        [HttpPut("{cityId}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(CityDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(NotFoundResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> UpdateCity(int cityId, [FromForm] UpdateCityCommand? command)
        {
            if (cityId <= 0) throw new NotFoundException("City not found!");
            if (command is null) return Ok();
            command.id = cityId;
            var city = await _mediator.Send(command);
            _logger.LogInformation("City with id '{cityId}' updated.", cityId);
            return Ok(city);
        }

        /// <summary>
        /// Deletes a city by its ID.
        /// </summary>
        /// <param name="cityId">The ID of the city to delete.</param>
        /// <returns>An <see cref="ActionResult{CityDto}"/> containing the deleted city details.</returns>
        /// <response code="200">Returns the deleted city details.</response>
        /// <response code="401">If the user is not authenticated.</response>
        /// <response code="403">If the user is not authorized.</response>
        /// <response code="404">If the city is not found.</response>
        /// <exception cref="NotFoundException">Thrown when the city is not found.</exception>
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

        /// <summary>
        /// Retrieves a list of the top visited cities.
        /// </summary>
        /// <returns>An <see cref="ActionResult{IEnumerable{CityTopDto}}"/> containing the list of top visited cities.</returns>
        /// <response code="200">Returns the list of top visited cities.</response>
        [HttpGet("top-visited-cities")]
        [ProducesResponseType(typeof(CityTopDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<CityTopDto>>> GetTopVisitedCities()
        {
            var command = new GetTopVisitedCitiesQuery();
            var cities = await _mediator.Send(command);
            return Ok(cities);
        }
    }
}
