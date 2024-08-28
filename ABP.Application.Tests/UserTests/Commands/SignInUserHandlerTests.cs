using Application.CommandsAndQueries.UserCQ.Commands.SignIn;
using Application.Dtos.UserDtos;
using AutoFixture;
using Domain.Abstractions;
using Domain.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Stripe;

namespace ABP.Application.Tests.UserTests.Commands
{
    public class SignInUserHandlerTests
    {
        private readonly Mock<UserManager<User>> _userManagerMock;
        private readonly Mock<SignInManager<User>> _signInManagerMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<ITokenService> _tokenServiceMock;
        private readonly SignInUserCommand _command;
        private readonly SignInUserHandler _handler;
        private readonly IFixture _fixture;

        public SignInUserHandlerTests()
        {
            var store = new Mock<IUserStore<User>>();
            _tokenServiceMock = new Mock<ITokenService>();
            _userManagerMock = new Mock<UserManager<User>>(
                store.Object, null, null, null, null, null, null, null, null);
            var contextAccessor = new Mock<IHttpContextAccessor>();
            var claimsFactory = new Mock<IUserClaimsPrincipalFactory<User>>();
            var options = new Mock<IOptions<IdentityOptions>>();
            var logger = new Mock<ILogger<SignInManager<User>>>();

            _signInManagerMock = new Mock<SignInManager<User>>(
                _userManagerMock.Object, contextAccessor.Object, claimsFactory.Object, options.Object, logger.Object, null, null);
            _configurationMock = new();
            _fixture = new Fixture();
            _command = new SignInUserCommand()
            {
                Password = _fixture.Create<Guid>().ToString(),
                UserName = _fixture.Create<string>()
            };

            _handler = new SignInUserHandler(
                _userManagerMock.Object, _signInManagerMock.Object, _configurationMock.Object, _tokenServiceMock.Object);
            _configurationMock.Setup(c => c.GetSection("JWTToken:Key").Value).Returns("This is Secret key do not share it");
            _configurationMock.Setup(c => c.GetSection("JWTToken:Issuer").Value).Returns("Issuer");
            _configurationMock.Setup(c => c.GetSection("JWTToken:Audience").Value).Returns("Audience");
        }
        [Fact]
        public async Task Handler_Should_ReturnTokenAndExpirationDateOfToken_WhenSuccess()
        {
            //Arrange
            _userManagerMock.Setup(x => x.FindByNameAsync(It.IsAny<string>())).ReturnsAsync(new User()
            {
                Id = _fixture.Create<Guid>().ToString(),
                UserName = _fixture.Create<string>(),
                Email = _fixture.Create<string>()
            });
            _userManagerMock.Setup(x => x.GetRolesAsync(It.IsAny<User>())).ReturnsAsync([]);
            _signInManagerMock.Setup(x => x.CheckPasswordSignInAsync(It.IsAny<User>(), It.IsAny<string>(), false)).ReturnsAsync(SignInResult.Success);
            //Act
            UserSignInDto? result = await _handler.Handle(_command, default);
            //Assert
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task Handler_Should_ReturnNull_WhenUsernameNotFound()
        {
            //Arrange
            _userManagerMock.Setup(x => x.FindByNameAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
            //Act
            UserSignInDto? result = await _handler.Handle(_command, default);
            //Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task Handler_Should_ReturnNull_WhenPasswordIsWrong()
        {
            //Arrange
            _userManagerMock.Setup(x => x.FindByNameAsync(It.IsAny<string>())).ReturnsAsync(new User()
            {
                Id = _fixture.Create<Guid>().ToString(),
                UserName = _fixture.Create<string>(),
                Email = _fixture.Create<string>()
            });
            _userManagerMock.Setup(x => x.GetRolesAsync(It.IsAny<User>())).ReturnsAsync([]);
            _signInManagerMock.Setup(x => x.CheckPasswordSignInAsync(It.IsAny<User>(), It.IsAny<string>(), false)).ReturnsAsync(SignInResult.Failed);
            //Act
            UserSignInDto? result = await _handler.Handle(_command, default);
            //Assert
            result.Should().BeNull();
        }
    }
}
