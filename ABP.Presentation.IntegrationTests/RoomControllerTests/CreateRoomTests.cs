using ABPIntegrationTests;
using Application.CommandsAndQueries.RoomCQ.Commands.Create;
using Application.Dtos.RoomDtos;
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

namespace ABP.Presentation.IntegrationTests.RoomControllerTests
{
    public class CreateRoomTests : IClassFixture<ABPWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly Fixture _fixture;
        private readonly ApplicationDbContext _dbContext;
        private readonly string _adminToken;
        private readonly string _userToken;
        private readonly CreateRoomCommand _command;

        public CreateRoomTests(ABPWebApplicationFactory factory)
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
            _fixture.Customize<Hotel>(x => x
                .Without(h => h.Reviews)
                .Without(h => h.RecentlyVisitedHotels)
                .Without(h => h.Rooms)
                .Without(h => h.SpecialOffers)
            );
            factory.SetupDbContext(_dbContext).GetAwaiter().GetResult();
            _adminToken = jwtTokenHelper.GetJwtTokenAsync("Admin").GetAwaiter().GetResult();
            _userToken = jwtTokenHelper.GetJwtTokenAsync("User").GetAwaiter().GetResult();
            _command = _fixture.Build<CreateRoomCommand>()
                .With(x => x.RoomNumber, _fixture.Create<string>()[0..10])
                .With(x => x.Thumbnail, GlobalTestData.GetFormFile())
                .With(x => x.Images, GlobalTestData.GetFormFiles(5))
                .Create();
        }

        [Fact]
        public async Task CreateRoom_Should_ReturnCreatedRoom_WhenSuccess()
        {
            // Arrange
            var hotel = _fixture.Create<Hotel>();
            await _dbContext.Hotels.AddAsync(hotel);
            await _dbContext.SaveChangesAsync();

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);
            var content = GlobalTestData.GetMultiPartFormDataFromCommand(_command);

            // Act
            var response = await _client.PostAsync($"/api/v1/hotels/{hotel.Id}/rooms", content);
            var createdRoom = response.Content.Headers.ContentType?.MediaType == "application/json" ?
                await response.Content.ReadFromJsonAsync<RoomDto>() : null;

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            createdRoom.Should().NotBeNull();
            createdRoom?.RoomNumber.Should().Be(_command.RoomNumber);
            createdRoom?.AdultCapacity.Should().Be(_command.AdultCapacity);
            createdRoom?.ChildrenCapacity.Should().Be(_command.ChildrenCapacity);
        }

        [Fact]
        public async Task CreateRoom_Should_ReturnUnauthorized_WhenTokenIsInvalidOrMissing()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "invalid token");
            var content = GlobalTestData.GetMultiPartFormDataFromCommand(_command);

            // Act
            var response = await _client.PostAsync($"/api/v1/hotels/1/rooms", content);

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task CreateRoom_Should_ReturnBadRequest_WhenRoomNumberIsEmpty()
        {
            // Arrange
            _command.RoomNumber = string.Empty;
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);
            var content = GlobalTestData.GetMultiPartFormDataFromCommand(_command);

            // Act
            var response = await _client.PostAsync($"/api/v1/hotels/1/rooms", content);

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateRoom_Should_ReturnBadRequest_WhenThumbnailIsEmpty()
        {
            // Arrange
            _command.Thumbnail = null!;
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);
            var content = GlobalTestData.GetMultiPartFormDataFromCommand(_command);

            // Act
            var response = await _client.PostAsync($"/api/v1/hotels/1/rooms", content);

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateRoom_Should_ReturnConflict_WhenRoomNumberExists()
        {
            // Arrange
            var hotel = _fixture.Create<Hotel>();
            var existingRoom = _fixture.Build<Room>()
                .With(x => x.RoomNumber, _command.RoomNumber)
                .With(x => x.HotelId, hotel.Id)
                .Without(x => x.Hotel)
                .Create();
            await _dbContext.Hotels.AddAsync(hotel);
            await _dbContext.Rooms.AddAsync(existingRoom);
            await _dbContext.SaveChangesAsync();

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);
            var content = GlobalTestData.GetMultiPartFormDataFromCommand(_command);

            // Act
            var response = await _client.PostAsync($"/api/v1/hotels/{hotel.Id}/rooms", content);

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }

        [Fact]
        public async Task CreateRoom_Should_ReturnForbidden_WhenUserIsNotAdmin()
        {
            // Arrange

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _userToken);
            var content = GlobalTestData.GetMultiPartFormDataFromCommand(_command);

            // Act
            var response = await _client.PostAsync($"/api/v1/hotels/1/rooms", content);

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task CreateRoom_Should_ReturnBadRequest_WhenAdultCapacityIsNegative()
        {
            // Arrange
            _command.AdultCapacity = -1;
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);
            var content = GlobalTestData.GetMultiPartFormDataFromCommand(_command);

            // Act
            var response = await _client.PostAsync($"/api/v1/hotels/1/rooms", content);

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateRoom_Should_ReturnBadRequest_WhenChildrenCapacityIsNegative()
        {
            // Arrange
            _command.ChildrenCapacity = -1;
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);
            var content = GlobalTestData.GetMultiPartFormDataFromCommand(_command);

            // Act
            var response = await _client.PostAsync($"/api/v1/hotels/1/rooms", content);

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateRoom_Should_ReturnBadRequest_WhenTooManyImages()
        {
            // Arrange
            _command.Images = GlobalTestData.GetFormFiles(21);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);
            var content = GlobalTestData.GetMultiPartFormDataFromCommand(_command);

            // Act
            var response = await _client.PostAsync($"/api/v1/hotels/1/rooms", content);

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateRoom_Should_ReturnBadRequest_WhenInvalidImageExtension()
        {
            // Arrange

            _command.Images = [ GlobalTestData.GetInvaildFormFile() ];
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);
            var content = GlobalTestData.GetMultiPartFormDataFromCommand(_command);

            // Act
            var response = await _client.PostAsync($"/api/v1/hotels/1/rooms", content);

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateRoom_Should_ReturnBadRequest_WhenInvalidThumbnailExtension()
        {
            // Arrange
            _command.Thumbnail = GlobalTestData.GetInvaildFormFile();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);
            var content = GlobalTestData.GetMultiPartFormDataFromCommand(_command);

            // Act
            var response = await _client.PostAsync($"/api/v1/hotels/1/rooms", content);

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
}
