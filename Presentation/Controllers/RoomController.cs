using Application.CommandsAndQueries.RoomCQ.Commands.Create;
using Application.CommandsAndQueries.RoomCQ.Commands.Delete;
using Application.Dtos.RoomDtos;
using Application.Exceptions;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Presentation.Responses.NotFound;
using Presentation.Responses.Validation;

namespace Presentation.Controllers
{
    /// <summary>
    /// Controller responsible for managing rooms within a specific hotel.
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/hotels/{hotelId}/rooms")]
    public class RoomController : ControllerBase
    {
        private readonly ILogger<RoomController> _logger;
        private readonly IMediator _mediator;

        /// <summary>
        /// Initializes a new instance of the <see cref="RoomController"/> class.
        /// </summary>
        /// <param name="logger">The logger instance for logging operations.</param>
        /// <param name="mediator">The mediator instance for handling commands.</param>
        /// <exception cref="ArgumentNullException">Thrown when the logger or mediator is null.</exception>
        public RoomController(ILogger<RoomController> logger, IMediator mediator)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        /// <summary>
        /// Creates a new room in the specified hotel.
        /// </summary>
        /// <param name="hotelId">The ID of the hotel where the room will be created.</param>
        /// <param name="command">The command containing the room details.</param>
        /// <returns>An <see cref="IActionResult"/> representing the result of the operation.</returns>
        /// <response code="201">If the room is successfully created.</response>
        /// <response code="400">If the request body is invalid.</response>
        /// <response code="401">If the user is not authenticated.</response>
        /// <response code="403">If the user is not authorized.</response>
        /// <response code="404">If the hotel is not found.</response>
        /// <response code="409">If the room Number already exist.</response>
        /// <exception cref="NotFoundException">Thrown when the hotel is not found.</exception>
        /// <exception cref="CustomValidationException">Thrown when the request body is invalid.</exception>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(RoomDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationFailureResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(NotFoundResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CreateRoom(int hotelId, [FromForm] CreateRoomCommand? command)
        {
            if (command is null) throw new CustomValidationException("The request must include a valid body.");
            if (hotelId <= 0) throw new NotFoundException("Hotel not found!");
            command.hotelId = hotelId;
            var room = await _mediator.Send(command);
            _logger.LogInformation(
                "New Room created: hotelid={hotelId}, RoomNumber={command.RoomNumber}",
                hotelId,
                command.RoomNumber
            );
            return CreatedAtRoute("GetHotelById", new { hotelId }, room);
        }

        /// <summary>
        /// Deletes a specific room from a hotel.
        /// </summary>
        /// <param name="hotelId">The ID of the hotel where the room is located.</param>
        /// <param name="roomNumber">The number of the room to be deleted.</param>
        /// <returns>An <see cref="IActionResult"/> representing the result of the operation.</returns>
        /// <response code="200">If the room is successfully deleted.</response>
        /// <response code="401">If the user is not authenticated.</response>
        /// <response code="403">If the user is not authorized.</response>
        /// <response code="404">If the hotel or room is not found.</response>
        /// <exception cref="NotFoundException">Thrown when the hotel or room is not found.</exception>
        [HttpDelete("{roomNumber}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(RoomDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(NotFoundResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> DeleteRoom(int hotelId, string roomNumber)
        {
            if (hotelId <= 0) throw new NotFoundException("Hotel not found!");
            var command = new DeleteRoomCommand() { HotelId = hotelId, RoomNumber = roomNumber };
            var roomDto = await _mediator.Send(command);
            _logger.LogInformation(
                "Room with ID '{roomId}' in the hotel with ID '{hotelId}' deleted",
                roomDto.RoomNumber,
                hotelId);
            return Ok(roomDto);
        }
    }
}
