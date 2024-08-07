using ABPIntegrationTests;
using Application.Dtos.RecentlyVisitedHotelDto;
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

namespace ABP.Presentation.IntegrationTests.UserControllerTests
{
    public class GetRecentlyVisitedHotelsTests : IClassFixture<ABPWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly Fixture _fixture;
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;
        private readonly string _userToken;
        private readonly string _adminToken;

        public GetRecentlyVisitedHotelsTests(ABPWebApplicationFactory factory)
        {
            factory.DatabaseName = Guid.NewGuid().ToString();
            _client = factory.CreateClient();
            _fixture = new Fixture();
            var scope = factory.Services.CreateScope();
            var serviceProvider = scope.ServiceProvider;
            _configuration = serviceProvider.GetRequiredService<IConfiguration>();
            _userManager = serviceProvider.GetRequiredService<UserManager<User>>();
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
            JwtTokenHelper jwtTokenHelper = new(_configuration, _userManager);
            _userToken = jwtTokenHelper.GetJwtTokenAsync("User").GetAwaiter().GetResult();
            _adminToken = jwtTokenHelper.GetJwtTokenAsync("Admin").GetAwaiter().GetResult();
        }

        [Fact]
        public async Task GetRecentlyVisitedHotels_Should_ReturnOk_WhenSuccess()
        {
            // Arrange
            var hotel = _fixture.Create<Hotel>();
            var recentlyVisitedHotel = new RecentlyVisitedHotel { UserId = "UserId", HotelId = hotel.Id };

            await _dbContext.Hotels.AddAsync(hotel);
            await _dbContext.RecentlyVisitedHotels.AddAsync(recentlyVisitedHotel);
            await _dbContext.SaveChangesAsync();

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _userToken);

            // Act
            var response = await _client.GetAsync($"/api/users/{"UserId"}/recently-visited-hotels");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<ICollection<RvhDto>>();
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetRecentlyVisitedHotels_Should_ReturnUnauthorized_WhenUserIsNotAuthenticated()
        {
            // Act
            var response = await _client.GetAsync("/api/users/test/recently-visited-hotels");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetRecentlyVisitedHotels_Should_ReturnForbidden_WhenUserAccessingOtherUsersHotels()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _userToken);

            // Act
            var response = await _client.GetAsync($"/api/users/{"User1Id"}/recently-visited-hotels");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task GetRecentlyVisitedHotels_Should_ReturnOk_WhenAdminAccessingAnyUsersHotels()
        {
            // Arrange
            var hotel = _fixture.Create<Hotel>();
            var recentlyVisitedHotel = new RecentlyVisitedHotel { UserId = "UserId", HotelId = hotel.Id };

            await _dbContext.Hotels.AddAsync(hotel);
            await _dbContext.RecentlyVisitedHotels.AddAsync(recentlyVisitedHotel);
            await _dbContext.SaveChangesAsync();

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);

            // Act
            var response = await _client.GetAsync($"/api/users/{"UserId"}/recently-visited-hotels");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<ICollection<RvhDto>>();
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
        }
    }
}
