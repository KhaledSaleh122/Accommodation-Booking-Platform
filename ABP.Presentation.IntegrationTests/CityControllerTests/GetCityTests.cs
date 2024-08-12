using ABPIntegrationTests;
using Application.Dtos.CityDtos;
using AutoFixture;
using Domain.Entities;
using FluentAssertions;
using Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;

namespace ABP.Presentation.IntegrationTests.CityControllerTests
{
    public class GetCityTests : IClassFixture<ABPWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly Fixture _fixture;
        private readonly ApplicationDbContext _dbContext;

        public GetCityTests(ABPWebApplicationFactory factory)
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
            factory.SetupDbContext(_dbContext).GetAwaiter().GetResult();
        }
        [Fact]
        public async Task GetCity_Should_ReturnCity_WhenSuccess()
        {
            // Arrange
            var city = _fixture.Build<City>()
                .Without(x => x.Hotels)
                .Create();
            await _dbContext.Cities.AddAsync(city);
            await _dbContext.SaveChangesAsync();
            // Act
            var response = await _client.GetAsync($"/api/v1/cities/{city.Id}");
            var returnedCity = response.Content.Headers.ContentType?.MediaType == "application/json" ?
                await response.Content.ReadFromJsonAsync<CityDto>() : null;
            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            returnedCity.Should().NotBeNull();
            returnedCity?.PostOffice.Should().Be(city.PostOffice);
            returnedCity?.Name.Should().Be(city.Name);
            returnedCity?.Country.Should().Be(city.Country);
        }

        [Fact]
        public async Task GetCity_Should_ReturnNotFound_WhenCityDoesNotExist()
        {
            // Act
            var response = await _client.GetAsync("/api/v1/cities/0");
            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

    }
}
