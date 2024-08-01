using ABPIntegrationTests;
using Application.CommandsAndQueries.AmenityCQ.Commands.Update;
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
    public class UpdateAmenityTests : IClassFixture<ABPWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly Fixture _fixture;
        private readonly ApplicationDbContext _dbContext;
        private readonly string _adminToken;
        private readonly string _userToken;

        public UpdateAmenityTests(ABPWebApplicationFactory factory)
        {
            factory.DatabaseName = $"InMemoryDb_Update";
            _client = factory.CreateClient();
            _fixture = new Fixture();
            var scope = factory.Services.CreateScope();
            var serviceProvider = scope.ServiceProvider;
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
            JwtTokenHelper jwtTokenHelper = new(configuration, userManager);
            _dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();
            _adminToken = jwtTokenHelper.GetJwtTokenAsync("Admin").GetAwaiter().GetResult();
            _userToken = jwtTokenHelper.GetJwtTokenAsync("User").GetAwaiter().GetResult();
        }
        [Fact]
        public async Task UpdateAmenity_Should_ReturnUpdatedAmenity_WhenSuccess()
        {
            // Arrange
            var amenity = _fixture.Build<Amenity>().With(c => c.Id, 10).Without(c => c.HotelAmenity).Create();
            await _dbContext.Amenities.AddAsync(amenity);
            await _dbContext.SaveChangesAsync();
            var command = _fixture.Build<UpdateAmenityCommand>().With(c => c.id, amenity.Id).Create();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);
            //Act
            var response = await _client.PutAsJsonAsync($"/api/amenities/{amenity.Id}", command);
            var returnedAmenity = response.Content.Headers.ContentType?.MediaType == "application/json" ?
                await response.Content.ReadFromJsonAsync<AmenityDto>() : null;
            //Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            returnedAmenity.Should().NotBeNull();
            returnedAmenity?.Id.Should().BeGreaterThanOrEqualTo(amenity.Id);
            returnedAmenity?.Name.Should().Be(command.Name);
            returnedAmenity?.Description.Should().Be(command.Description);

        }

        [Fact]
        public async Task UpdateAmenity_Should_ReturnUnauthorized_WhenTokenIsInvalidOrMissing()
        {

            // Arrange
            var command = _fixture.Create<UpdateAmenityCommand>();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "invaild token");
            //Act
            var response = await _client.PostAsJsonAsync("/api/amenities", command);
            //Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task UpdateAmenity_Should_ReturnNotFound_WhenAmenityDoesNotExist()
        {
            //Arrange
            var command = _fixture.Create<UpdateAmenityCommand>();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);
            //Act
            var response = await _client.PutAsJsonAsync($"/api/amenities/0", command);
            //Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task UpdateAmenity_Should_ReturnBadRequest_WhenNameIsEmpty()
        {
            // Arrange
            var command = _fixture.Build<UpdateAmenityCommand>()
                .With(x => x.Name, string.Empty)
                .Create();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);

            // Act
            var response = await _client.PutAsJsonAsync("/api/amenities/1", command);

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task UpdateAmenity_Should_ReturnBadRequest_WhenDescriptionIsEmpty()
        {
            // Arrange
            var command = _fixture.Build<UpdateAmenityCommand>()
                .With(x => x.Description, string.Empty)
                .Create();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);

            // Act
            var response = await _client.PutAsJsonAsync("/api/amenities/1", command);

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task UpdateAmenity_Should_ReturnBadRequest_WhenNameExceedsMaxLength()
        {
            // Arrange
            var command = _fixture.Build<UpdateAmenityCommand>()
                .With(x => x.Name, new string('A', 61))
                .Create();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);

            // Act
            var response = await _client.PutAsJsonAsync("/api/amenities/1", command);

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task UpdateAmenity_Should_ReturnBadRequest_WhenDescriptionExceedsMaxLength()
        {
            // Arrange
            var command = _fixture.Build<UpdateAmenityCommand>()
                .With(x => x.Description, new string('A', 161))
                .Create();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);

            // Act
            var response = await _client.PutAsJsonAsync("/api/amenities/1", command);

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task UpdateAmenity_Should_ReturnForbidden_WhenUserIsNotAdmin()
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
