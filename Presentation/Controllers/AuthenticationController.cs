using Application.CommandsAndQueries.UserCQ.Commands.Create;
using Application.CommandsAndQueries.UserCQ.Commands.SignIn;
using Application.CommandsAndQueries.UserCQ.Commands.SignInGoogle;
using Application.Dtos.UserDtos;
using Application.Exceptions;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Presentation.Responses.Validation;

namespace Presentation.Controllers
{
    /// <summary>
    /// Controller responsible for handling authentication-related operations.
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}")]
    [Authorize(Policy = "Guest")]
    public class AuthenticationController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<AmenityController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationController"/> class.
        /// </summary>
        /// <param name="mediator">The mediator instance for handling commands.</param>
        /// <param name="logger">The logger instance for logging operations.</param>
        /// <exception cref="ArgumentNullException">Thrown when the mediator or logger is null.</exception>
        public AuthenticationController(IMediator mediator, ILogger<AmenityController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Registers a new user.
        /// </summary>
        /// <param name="command">The command containing user registration details.</param>
        /// <returns>An <see cref="IActionResult"/> representing the result of the operation.</returns>
        /// <response code="201">Returns the created user.</response>
        /// <response code="400">If the command is null or validation fails.</response>
        /// <response code="403">If the user is not authorized.</response>
        /// <response code="409">If the user details exist.</response>
        /// <exception cref="CustomValidationException">Thrown when the command is null.</exception>
        [HttpPost("users")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationFailureResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RegisterUser([FromForm] CreateUserCommand? command)
        {
            if (command is null) throw new CustomValidationException("The body for this request is required.");
            var createdUser = await _mediator.Send(command);
            _logger.LogInformation(
                "New Account Created: Name '{command.UserName}', Email '{command.Email}'",
                command.UserName,
                command.Email
            );
            return new ObjectResult(createdUser)
            {
                StatusCode = StatusCodes.Status201Created
            };
        }

        /// <summary>
        /// Logs in a user.
        /// </summary>
        /// <param name="command">The command containing user login details.</param>
        /// <returns>An <see cref="IActionResult"/> representing the result of the operation.</returns>
        /// <response code="200">Returns the signed-in user details.</response>
        /// <response code="400">If the command is null or validation fails.</response>
        /// <response code="403">If the user is not authorized.</response>
        [HttpPost("sessions")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(UserSignInDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationFailureResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> LoginInUser(SignInUserCommand? command)
        {
            if (command is null)
                throw new CustomValidationException("The body for this request is required.");
            var signInDto = await _mediator.Send(command);
            if (signInDto is null) return Unauthorized();
            _logger.LogInformation(
                "New login to account: Name '{command.UserName}'",
                command.UserName
            );
            return Ok(signInDto);
        }

        /// <summary>
        /// Handles the Google login response.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> representing the result of the operation.</returns>
        /// <response code="200">Returns the signed-in user details.</response>
        /// <response code="400">If the validation fails.</response>
        /// <response code="403">If the user is not authorized.</response>
        /// <response code="409">If the user details exist.</response>
        [HttpGet("sessions/google-response")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(UserSignInDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationFailureResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GoogleResponse()
        {
            var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
            if (result.Principal is null) return Unauthorized();

            var signInInformation = await _mediator.Send(new SignInGoogleCommand(result.Principal.Claims));
            return Ok(signInInformation);
        }

        /// <summary>
        /// Initiates the Google sign-in process.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> that challenges the user to sign in with Google.</returns>
        [HttpGet("sessions/google")]
        public IActionResult SignInWithGoogle()
        {
            var redirectUrl = Url.Action(nameof(GoogleResponse));
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }
    }
}
