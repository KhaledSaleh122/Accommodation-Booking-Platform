using Application.Dtos.UserDtos;
using Domain.Abstractions;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace Application.CommandsAndQueries.UserCQ.Commands.SignIn
{
    internal class SignInUserHandler : IRequestHandler<SignInUserCommand, UserSignInDto?>
    {
        private readonly IUserManager _userManager;
        private readonly ISignInManager _signInManager;
        private readonly IConfiguration _configuration;
        private readonly ITokenService _tokenService;

        public SignInUserHandler(
            IUserManager userManager,
            ISignInManager signInManager,
            IConfiguration configuration,
            ITokenService tokenService)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
        }

        public async Task<UserSignInDto?> Handle(SignInUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByNameAsync(request.UserName);
            if (user is null) return null;
            var success = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
            if (!success)
                return null;

            var roles = await _userManager.GetRolesAsync(user);

            var (token, expireDate) = _tokenService.GenerateUserToken(user, roles);
            return new UserSignInDto() { Token = token, Expiration = expireDate };
        }
    }
}
