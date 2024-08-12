using ABPIntegrationTests;
using Application.Dtos.HotelDtos;
using AutoFixture;
using Domain.Entities;
using FluentAssertions;
using Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;

namespace ABP.Presentation.IntegrationTests.HotelControllerTests
{
    public class GetHotelTests : IClassFixture<ABPWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly Fixture _fixture;
        private readonly ApplicationDbContext _dbContext;

        public GetHotelTests(ABPWebApplicationFactory factory)
        {
            factory.DatabaseName = Guid.NewGuid().ToString();
            _client = factory.CreateClient();
            _fixture = new Fixture();
            var scope = factory.Services.CreateScope();
            var serviceProvider = scope.ServiceProvider;
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            _fixture.Customize<DateOnly>(composer => composer.FromFactory<int, int, int>((year, month, day) =>
            {
                var now = DateTime.Now;
                return new DateOnly(now.Year, now.Month, now.Day);
            }));
            JwtTokenHelper jwtTokenHelper = new(configuration, userManager);
            _dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();
            factory.SetupDbContext(_dbContext).GetAwaiter().GetResult();
        }
        [Fact]
        public async Task GetHotel_Should_ReturnHotel_WhenSuccess()
        {
            // Arrange
            var hotel = _fixture.Build<Hotel>().Create();
            await _dbContext.Hotels.AddAsync(hotel);
            await _dbContext.SaveChangesAsync();
            // Act
            var response = await _client.GetAsync($"/api/v1/hotels/{hotel.Id}");
            var returnedHotel = response.Content.Headers.ContentType?.MediaType == "application/json" ?
                await response.Content.ReadFromJsonAsync<HotelDto>() : null;
            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            returnedHotel.Should().NotBeNull();
            returnedHotel?.Id.Should().BeGreaterThanOrEqualTo(1);
            returnedHotel?.Name.Should().Be(hotel.Name);
            returnedHotel?.Description.Should().Be(hotel.Description);
            returnedHotel?.Address.Should().Be(hotel.Address);
            returnedHotel?.HotelType.Should().Be(hotel.HotelType.ToString());
            returnedHotel?.Owner.Should().Be(hotel.Owner);
            returnedHotel?.PricePerNight.Should().Be(hotel.PricePerNight);
            returnedHotel?.City.Should().Be(hotel.City.Name);
        }

        [Fact]
        public async Task GetHotel_Should_ReturnNotFound_WhenHotelDoesNotExist()
        {
            // Act
            var response = await _client.GetAsync("/api/v1/hotels/0");
            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

    }
}
