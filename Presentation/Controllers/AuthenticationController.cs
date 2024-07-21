﻿using Application.CommandsAndQueries.UserCQ.Commands.Create;
using Application.CommandsAndQueries.UserCQ.Commands.SignIn;
using Application.Dtos.UserDtos;
using Application.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Presentation.Responses.Validation;

namespace Presentation.Controllers
{
    [ApiController]
    [Route("api")]
    public class AuthenticationController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<AmenityController> _logger;
        public AuthenticationController(IMediator mediator, ILogger<AmenityController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpPost("users")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationFailureResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RegisterUser(CreateUserCommand? command)
        {
            if (command is null) throw new CustomValidationException("The body for this request required");
            var createdUser = await _mediator.Send(command);
            _logger.LogInformation(
                "New Account Created name '{command.UserName}',  Email '{command.Email}'",
                command.UserName,
                command.Email
            );
            return new ObjectResult(createdUser)
            {
                StatusCode = StatusCodes.Status201Created
            };
        }
        [HttpPost("sessions")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationFailureResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> LoginInUser(SignInUserCommand? command)
        {
            if (command is null) 
                throw new CustomValidationException("The body for this request required");
            var token = await _mediator.Send(command);
            _logger.LogInformation(
                "New login to account name '{command.UserName}'",
                command.UserName
            );
            return Ok(token);
        }

    }
}
