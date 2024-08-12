using Application.CommandsAndQueries.SpecialOfferCQ.Commands.Create;
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
    /// <summary>
    /// Controller responsible for managing special offers related to hotels.
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}")]
    public class SpecialOfferController : ControllerBase
    {
        private readonly ILogger<SpecialOfferController> _logger;
        private readonly IMediator _mediator;

        /// <summary>
        /// Initializes a new instance of the <see cref="SpecialOfferController"/> class.
        /// </summary>
        /// <param name="logger">The logger instance for logging operations.</param>
        /// <param name="mediator">The mediator instance for handling commands.</param>
        /// <exception cref="ArgumentNullException">Thrown when the logger or mediator is null.</exception>
        public SpecialOfferController(ILogger<SpecialOfferController> logger, IMediator mediator)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        /// <summary>
        /// Creates a new special offer for a specific hotel.
        /// </summary>
        /// <param name="hotelId">The ID of the hotel to which the special offer will be added.</param>
        /// <param name="command">The command containing the special offer details.</param>
        /// <returns>An <see cref="IActionResult"/> representing the result of the operation.</returns>
        /// <response code="200">If the special offer is successfully created.</response>
        /// <response code="400">If the request body is invalid.</response>
        /// <response code="401">If the user is not authenticated.</response>
        /// <response code="403">If the user is not authorized.</response>
        /// <response code="404">If the hotel is not found.</response>
        /// <response code="409">If a special offer with this id already exists.</response>
        /// <exception cref="NotFoundException">Thrown when the hotel is not found.</exception>
        /// <exception cref="CustomValidationException">Thrown when the request body is invalid.</exception>
        [HttpPost("hotels/{hotelId}/special-offers")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ICollection<SpecialOfferDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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

        /// <summary>
        /// Retrieves the top special feature deal offers.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> containing a collection of top special feature deal offers.</returns>
        /// <response code="200">If the top special feature deal offers are successfully retrieved.</response>
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
