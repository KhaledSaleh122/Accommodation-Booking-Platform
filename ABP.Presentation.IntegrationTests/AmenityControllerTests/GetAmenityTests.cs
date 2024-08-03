using ABPIntegrationTests;
using Application.Dtos.AmenityDtos;
using AutoFixture;
using Domain.Entities;
using FluentAssertions;
using Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;

namespace ABP.Presentation.IntegrationTests.AmenityControllerTests
{
    public class GetAmenityTests : IClassFixture<ABPWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly Fixture _fixture;
        private readonly ApplicationDbContext _dbContext;

        public GetAmenityTests(ABPWebApplicationFactory factory)
        {
            factory.DatabaseName = Guid.NewGuid().ToString();
            _client = factory.CreateClient();
            _fixture = new Fixture();
            _dbContext = factory.Services.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();
            factory.SetupDbContext(_dbContext).GetAwaiter().GetResult();
        }

        [Fact]
        public async Task GetAmenity_Should_ReturnAmenity_WhenSuccess()
        {
            // Arrange
            var amenity = _fixture.Build<Amenity>().With(c => c.Id, 10).Without(c => c.HotelAmenity).Create();
            await _dbContext.Amenities.AddAsync(amenity);
            await _dbContext.SaveChangesAsync();
            // Act
            var response = await _client.GetAsync($"/api/amenities/{amenity.Id}");
            var returnedAmenity = response.Content.Headers.ContentType?.MediaType == "application/json" ?
                await response.Content.ReadFromJsonAsync<AmenityDto>() : null;

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            returnedAmenity.Should().NotBeNull();
            returnedAmenity?.Id.Should().Be(amenity.Id);
            returnedAmenity?.Name.Should().Be(amenity.Name);
            returnedAmenity?.Description.Should().Be(amenity.Description);
        }

        [Fact]
        public async Task GetAmenity_Should_ReturnNotFound_WhenAmenityDoesNotExist()
        {
            // Act
            var response = await _client.GetAsync("/api/amenities/0");
            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}
