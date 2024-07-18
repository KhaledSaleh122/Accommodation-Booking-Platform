﻿using Application.CommandsAndQueries.CityCQ.Query.GetCities;
using Application.CommandsAndQueries.CityCQ.Query.GetCityById;
using Application.Dtos.CityDtos;
using Application.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Presentation.Responses.NotFound;
using Presentation.Responses.Pagination;

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
        public async Task<ActionResult<CityDto>> GetCity(uint? cityId)
        {
            if (cityId is null) throw new NotFoundException("City not found!");
            var query = new GetCityByIdQuery((uint)cityId);
            var cityDto = await _mediator.Send(query);
            if (cityDto is null) return NotFound();
            return Ok(cityDto);
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<CityDto>>> GetCities(
                string? city,
                string? country,
                uint page = 1,
                uint pageSize = 10
            )
        {
            var query = new GetCitiesQuery()
            {
                Page = page,
                PageSize = pageSize,
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
    }
}
