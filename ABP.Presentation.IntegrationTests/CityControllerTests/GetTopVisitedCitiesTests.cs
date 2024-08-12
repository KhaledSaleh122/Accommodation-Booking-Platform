using ABPIntegrationTests;
using Application.Dtos.CityDtos;
using AutoFixture;
using Domain.Entities;
using FluentAssertions;
using Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;

namespace ABP.Presentation.IntegrationTests.CityControllerTests
{
    public class GetTopVisitedCitiesTests : IClassFixture<ABPWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly Fixture _fixture;
        private readonly ApplicationDbContext _dbContext;

        public GetTopVisitedCitiesTests(ABPWebApplicationFactory factory)
        {
            factory.DatabaseName = Guid.NewGuid().ToString();
            _client = factory.CreateClient();
            _fixture = new Fixture();
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            _fixture.Customize<DateOnly>(composer => composer.FromFactory<int, int, int>((year, month, day) =>
            {
                var now = DateTime.Now;
                return new DateOnly(now.Year, now.Month, now.Day);
            }));
            var scope = factory.Services.CreateScope();
            _dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            factory.SetupDbContext(_dbContext).GetAwaiter().GetResult();
        }

        [Fact]
        public async Task GetTopVisitedCities_Should_ReturnTopVisitedCities_WhenSuccess()
        {
            // Arrange
            var cities = _fixture.Build<City>()
                .CreateMany(6);
            foreach (var hotel in cities.ElementAt(5).Hotels)
            {
                hotel.RecentlyVisitedHotels = [];
            }
            var topCities = cities.Take(5).ToList();
            await _dbContext.Cities.AddRangeAsync(cities);
            await _dbContext.SaveChangesAsync();

            // Act
            var response = await _client.GetAsync("/api/v1/cities/top-visited-cities");
            var result = response.Content.Headers.ContentType?.MediaType == "application/json" ?
                await response.Content.ReadFromJsonAsync<IEnumerable<CityTopDto>>() : null;

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            result.Should().NotBeNull();
            result.Should().HaveCount(5);
            result.Should().Contain(x => topCities.Any(y => y.Id == x.Id));
        }


        [Fact]
        public async Task GetTopVisitedCities_Should_ReturnEmptyList_WhenNoCitiesExist()
        {
            // Act
            var response = await _client.GetAsync("/api/v1/cities/top-visited-cities");
            var result = response.Content.Headers.ContentType?.MediaType == "application/json" ?
                await response.Content.ReadFromJsonAsync<IEnumerable<CityTopDto>>() : null;

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }
    }
}
