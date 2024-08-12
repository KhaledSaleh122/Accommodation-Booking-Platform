using ABPIntegrationTests;
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
    public class DeleteRoomTests : IClassFixture<ABPWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly Fixture _fixture;
        private readonly ApplicationDbContext _dbContext;
        private readonly string _adminToken;
        private readonly string _userToken;

        public DeleteRoomTests(ABPWebApplicationFactory factory)
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
        }

        [Fact]
        public async Task DeleteRoom_Should_ReturnOk_WhenSuccess()
        {
            // Arrange
            var hotel = _fixture.Create<Hotel>();
            var room = _fixture.Build<Room>()
                .With(x => x.HotelId, hotel.Id)
                .Without(x => x.Hotel)
                .Create();
            await _dbContext.Hotels.AddAsync(hotel);
            await _dbContext.Rooms.AddAsync(room);
            await _dbContext.SaveChangesAsync();

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);

            // Act
            var response = await _client.DeleteAsync($"/api/v1/hotels/{hotel.Id}/rooms/{room.RoomNumber}");
            var deletedRoom = await response.Content.ReadFromJsonAsync<RoomDto>();

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            deletedRoom.Should().NotBeNull();
            deletedRoom?.RoomNumber.Should().Be(room.RoomNumber);
            deletedRoom?.AdultCapacity.Should().Be(room.AdultCapacity);
            deletedRoom?.ChildrenCapacity.Should().Be(room.ChildrenCapacity);
        }

        [Fact]
        public async Task DeleteRoom_Should_ReturnUnauthorized_WhenTokenIsInvalidOrMissing()
        {
            // Arrange

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "invalid token");
            // Act
            var response = await _client.DeleteAsync($"/api/v1/hotels/1/rooms/01");

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task DeleteRoom_Should_ReturnForbidden_WhenUserIsNotAdmin()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _userToken);

            // Act
            var response = await _client.DeleteAsync("/api/v1/hotels/1/rooms/01");

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task DeleteRoom_Should_ReturnNotFound_WhenHotelDoesNotExist()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);
            var url = $"/api/v1/hotels/0/rooms/01";

            // Act
            var response = await _client.DeleteAsync(url);

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task DeleteRoom_Should_ReturnNotFound_WhenRoomDoesNotExist()
        {
            // Arrange
            var hotel = _fixture.Create<Hotel>();
            await _dbContext.Hotels.AddAsync(hotel);
            await _dbContext.SaveChangesAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);
            var url = $"/api/v1/hotels/{hotel.Id}/rooms/01";

            // Act
            var response = await _client.DeleteAsync(url);

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}
