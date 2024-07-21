using Application.CommandsAndQueries.RoomCQ.Commands.Create;
using Application.CommandsAndQueries.RoomCQ.Commands.Delete;
using Application.Dtos.RoomDtos;
using Application.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Presentation.Responses.NotFound;

namespace Presentation.Controllers
{
    [ApiController]
    [Route("api/hotels/{hotelId}/rooms")]
    public class RoomController : ControllerBase
    {
        private readonly ILogger<RoomController> _logger;
        private readonly IMediator _mediator;
        public RoomController(ILogger<RoomController> logger, IMediator mediator)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [HttpPost]
        [ProducesResponseType(typeof(RoomDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(NotFoundResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreateRoom(int hotelId, [FromForm] CreateRoomCommand? command)
        {
            if (command is null) throw new CustomValidationException("The request must include a body.");
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

        [HttpDelete("{roomNumber}")]
        [ProducesResponseType(typeof(RoomDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(NotFoundResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteRoom(int hotelId, string roomNumber)
        {
            if (hotelId <= 0) throw new NotFoundException("Hotel not found!");
            var command = new DeleteRoomCommand() { HotelId = hotelId, RoomNumber = roomNumber };
            var roomDto = await _mediator.Send(command);
            return Ok(roomDto);
        }
    }
}
