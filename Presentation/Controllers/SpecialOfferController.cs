﻿using Application.CommandsAndQueries.SpecialOfferCQ.Commands.Create;
using Application.CommandsAndQueries.SpecialOfferCQ.Queries.GetTopFeatureDealOffers;
using Application.Dtos.SpecialOfferDtos;
using Application.Exceptions;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Presentation.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}")]
    public class SpecialOfferController : ControllerBase
    {
        private readonly ILogger<SpecialOfferController> _logger;
        private readonly IMediator _mediator;
        public SpecialOfferController(ILogger<SpecialOfferController> logger, IMediator mediator)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [HttpPost("hotels/{hotelId}/special-offers")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ICollection<SpecialOfferDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CreateSpecialOffer(int hotelId, CreateSpecialOfferCommand? command)
        {
            if (command is null) throw new CustomValidationException("The request must include a valid body.");
            if (hotelId <= 0) throw new NotFoundException("Hotel not found!");
            command.hotelId = hotelId;
            var specialOffer = await _mediator.Send(command);
            _logger.LogInformation(
                "New special offer created with id '{specialOfferId}' for hotel with ID '{hotelId}'",
                specialOffer.Id,
                hotelId);
            return Ok(specialOffer);
        }
        [HttpGet("special-offers")]
        [ProducesResponseType(typeof(ICollection<FeaturedDealsDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTopSpecialFeatureDealOffers()
        {
            var command = new GetTopSpecialFeatureOffersQuery();
            var topSpecial = await _mediator.Send(command);
            return Ok(topSpecial);
        }
    }
}
