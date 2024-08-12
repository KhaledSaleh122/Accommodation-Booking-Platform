using ABPIntegrationTests;
using Application.CommandsAndQueries.UserCQ.Commands.Create;
using Application.Dtos.UserDtos;
using AutoFixture;
using Domain.Entities;
using FluentAssertions;
using Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Mail;

namespace ABP.Presentation.IntegrationTests.AuthenticationControllerTests
{
    public class RegisterUserTests : IClassFixture<ABPWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly Fixture _fixture;
        private readonly ApplicationDbContext _dbContext;
        private readonly string _adminToken;
        private readonly string _userToken;
        private readonly CreateUserCommand _command;

        public RegisterUserTests(ABPWebApplicationFactory factory)
        {
            factory.DatabaseName = Guid.NewGuid().ToString();
            _client = factory.CreateClient();
            _fixture = new Fixture();
            var scope = factory.Services.CreateScope();
            var serviceProvider = scope.ServiceProvider;
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
            JwtTokenHelper jwtTokenHelper = new(configuration, userManager);
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
            _command = _fixture.Build<CreateUserCommand>()
                .With(x => x.UserName, _fixture.Create<string>()[0..10])
                .With(x => x.Email, _fixture.Create<MailAddress>().Address)
                .With(x => x.Thumbnail, GlobalTestData.GetFormFile())
                .Create();
        }

        [Fact]
        public async Task RegisterUser_Should_ReturnCreatedUser_WhenSuccess()
        {
            // Arrange
            var content = GlobalTestData.GetMultiPartFormDataFromCommand(_command);

            // Act
            var response = await _client.PostAsync($"/api/v1/users", content);
            var createdUser = response.Content.Headers.ContentType?.MediaType == "application/json" ?
                await response.Content.ReadFromJsonAsync<UserDto>() : null;

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            createdUser.Should().NotBeNull();
            createdUser?.UserName.Should().Be(_command.UserName);
            createdUser?.Email.Should().Be(_command.Email);
        }

        [Fact]
        public async Task RegisterUser_Should_ReturnBadRequest_WhenUserNameIsEmpty()
        {
            // Arrange
            _command.UserName = string.Empty;
            var content = GlobalTestData.GetMultiPartFormDataFromCommand(_command);

            // Act
            var response = await _client.PostAsync($"/api/v1/users", content);

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task RegisterUser_Should_ReturnBadRequest_WhenThumbnailIsEmpty()
        {
            // Arrange
            _command.Thumbnail = null!;
            var content = GlobalTestData.GetMultiPartFormDataFromCommand(_command);

            // Act
            var response = await _client.PostAsync($"/api/v1/users", content);

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task RegisterUser_Should_ReturnConflict_WhenEmailExists()
        {
            // Arrange
            var existingUser = _fixture.Create<User>();
            existingUser.Email = _command.Email;
            existingUser.NormalizedEmail = _command.Email.ToUpper();
            await _dbContext.Users.AddAsync(existingUser);
            await _dbContext.SaveChangesAsync();
            var users = await _dbContext.Users.ToListAsync();
            var content = GlobalTestData.GetMultiPartFormDataFromCommand(_command);

            // Act
            var response = await _client.PostAsync($"/api/v1/users", content);

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }

        [Fact]
        public async Task RegisterUser_Should_ReturnConflict_WhenUserNameExists()
        {
            // Arrange
            var existingUser = _fixture.Create<User>();
            existingUser.UserName = _command.UserName;
            existingUser.NormalizedUserName = _command.UserName.ToUpper();
            await _dbContext.Users.AddAsync(existingUser);
            await _dbContext.SaveChangesAsync();

            var content = GlobalTestData.GetMultiPartFormDataFromCommand(_command);

            // Act
            var response = await _client.PostAsync($"/api/v1/users", content);

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }

        [Fact]
        public async Task RegisterUser_Should_ReturnForbidden_WhenAuthenticatedUserSendRequest()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _userToken);
            var content = GlobalTestData.GetMultiPartFormDataFromCommand(_command);

            // Act
            var response = await _client.PostAsync($"/api/v1/users", content);

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task RegisterUser_Should_ReturnForbidden_WhenAuthenticatedAdminSendRequest()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);
            var content = GlobalTestData.GetMultiPartFormDataFromCommand(_command);

            // Act
            var response = await _client.PostAsync($"/api/v1/users", content);

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task RegisterUser_Should_ReturnBadRequest_WhenPasswordIsTooShort()
        {
            // Arrange
            _command.Password = "short";
            var content = GlobalTestData.GetMultiPartFormDataFromCommand(_command);

            // Act
            var response = await _client.PostAsync($"/api/v1/users", content);

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task RegisterUser_Should_ReturnBadRequest_WhenInvalidEmailFormat()
        {
            // Arrange
            _command.Email = "invalid-email";
            var content = GlobalTestData.GetMultiPartFormDataFromCommand(_command);

            // Act
            var response = await _client.PostAsync($"/api/v1/users", content);

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task RegisterUser_Should_ReturnBadRequest_WhenInvalidThumbnailExtension()
        {
            // Arrange
            _command.Thumbnail = GlobalTestData.GetInvaildFormFile();
            var content = GlobalTestData.GetMultiPartFormDataFromCommand(_command);

            // Act
            var response = await _client.PostAsync($"/api/v1/users", content);

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
}
