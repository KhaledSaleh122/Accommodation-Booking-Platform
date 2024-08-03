using ABPIntegrationTests;
using Application.Dtos.AmenityDtos;
using AutoFixture;
using Domain.Entities;
using FluentAssertions;
using Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace ABP.Presentation.IntegrationTests.AmenityControllerTests
{
    public class DeleteAmenityTests : IClassFixture<ABPWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly Fixture _fixture;
        private readonly ApplicationDbContext _dbContext;
        private readonly string _adminToken;
        private readonly string _userToken;

        public DeleteAmenityTests(ABPWebApplicationFactory factory)
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
            _adminToken = jwtTokenHelper.GetJwtTokenAsync("Admin").GetAwaiter().GetResult();
            _userToken = jwtTokenHelper.GetJwtTokenAsync("User").GetAwaiter().GetResult();
        }

        [Fact]
        public async Task DeleteAmenity_Should_ReturnDeletedAmenity_WhenSuccess()
        {
            // Arrange
            var amenity = _fixture.Build<Amenity>().With(c => c.Id, 10).Without(c => c.HotelAmenity).Create();
            await _dbContext.Amenities.AddAsync(amenity);
            await _dbContext.SaveChangesAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);

            // Act
            var response = await _client.DeleteAsync($"/api/amenities/{amenity.Id}");
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
        public async Task DeleteAmenity_Should_ReturnUnauthorized_WhenTokenIsInvalidOrMissing()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "invalid token");

            // Act
            var response = await _client.DeleteAsync("/api/amenities/1");

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task DeleteAmenity_Should_ReturnNotFound_WhenAmenityDoesNotExist()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);

            // Act
            var response = await _client.DeleteAsync("/api/amenities/999");

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task DeleteAmenity_Should_ReturnForbidden_WhenUserIsNotAdmin()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _userToken);

            // Act
            var response = await _client.DeleteAsync($"/api/cities/1");

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }
    }
}
