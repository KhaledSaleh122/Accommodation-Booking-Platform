using ABPIntegrationTests;
using Application.CommandsAndQueries.UserCQ.Commands.SignIn;
using Application.Dtos.UserDtos;
using AutoFixture;
using Domain.Entities;
using FluentAssertions;
using Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace ABP.Presentation.IntegrationTests.AuthenticationControllerTests
{
    public class SignInUserTests : IClassFixture<ABPWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly Fixture _fixture;
        private readonly ApplicationDbContext _dbContext;
        private readonly string _adminToken;
        private readonly string _userToken;
        private readonly SignInUserCommand _command;
        private readonly UserManager<User> _userManager;

        public SignInUserTests(ABPWebApplicationFactory factory)
        {
            factory.DatabaseName = Guid.NewGuid().ToString();
            _client = factory.CreateClient();
            _fixture = new Fixture();
            var scope = factory.Services.CreateScope();
            var serviceProvider = scope.ServiceProvider;
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            _userManager = serviceProvider.GetRequiredService<UserManager<User>>();
            JwtTokenHelper jwtTokenHelper = new(configuration, _userManager);
            _dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            _fixture.Customize<DateOnly>(composer => composer.FromFactory<int, int, int>((year, month, day) =>
            {
                var now = DateTime.Now;
                return new DateOnly(now.Year, now.Month, now.Day);
            }));
            _fixture.Customize<User>(x => x
                .Without(u => u.Reviews)
                .Without(u => u.RecentlyVisitedHotels)
                .Without(u => u.Bookings)
            );
            factory.SetupDbContext(_dbContext).GetAwaiter().GetResult();
            _adminToken = jwtTokenHelper.GetJwtTokenAsync("Admin").GetAwaiter().GetResult();
            _userToken = jwtTokenHelper.GetJwtTokenAsync("User").GetAwaiter().GetResult();
            _command = _fixture.Build<SignInUserCommand>()
                .With(x => x.UserName, _fixture.Create<string>()[0..10])
                .With(x => x.Password, _fixture.Create<string>())
                .Create();
        }

        [Fact]
        public async Task SignInUser_Should_ReturnOk_WhenCredentialsAreCorrect()
        {
            // Arrange
            var user = new User { UserName = _command.UserName, Email = "testuser@example.com", Thumbnail = "test.png" };
            await _userManager.CreateAsync(user, _command.Password);
            await _userManager.AddToRoleAsync(user, "User");

            // Act
            var response = await _client.PostAsJsonAsync("/api/sessions", _command);
            var signInDto = response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<UserSignInDto>()
                : null;

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            signInDto.Should().NotBeNull();
            signInDto?.Token.Should().NotBeNullOrEmpty();
            signInDto?.Expiration.Should().BeAfter(DateTime.UtcNow);
        }

        [Fact]
        public async Task SignInUser_Should_ReturnUnauthorized_WhenCredentialsAreIncorrect()
        {
            // Arrange
            var user = new User { UserName = _command.UserName, Email = "testuser@example.com", Thumbnail = "test.png" };
            await _userManager.CreateAsync(user, "WrongPassword");
            await _userManager.AddToRoleAsync(user, "User");


            // Act
            var response = await _client.PostAsJsonAsync("/api/sessions", _command);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task SignInUser_Should_ReturnBadRequest_WhenUserNameIsEmpty()
        {
            // Arrange
            _command.UserName = string.Empty;

            // Act
            var response = await _client.PostAsJsonAsync("/api/sessions", _command);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task SignInUser_Should_ReturnBadRequest_WhenPasswordIsEmpty()
        {
            // Arrange
            _command.Password = string.Empty;

            // Act
            var response = await _client.PostAsJsonAsync("/api/sessions", _command);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task SignInUser_Should_ReturnBadRequest_WhenCommandIsNull()
        {
            // Act
            var response = await _client.PostAsync("/api/sessions", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task SignInUser_Should_ReturnForbidden_WhenAuthenticatedUserSendRequest()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _userToken);

            // Act
            var response = await _client.PostAsJsonAsync($"/api/sessions", _command);

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task SignInUser_Should_ReturnForbidden_WhenAuthenticatedAdminSendRequest()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);

            // Act
            var response = await _client.PostAsJsonAsync($"/api/sessions", _command);

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }
    }
}