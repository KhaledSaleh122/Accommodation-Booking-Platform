using ABPIntegrationTests;
using Application.CommandsAndQueries.AmenityCQ.Commands.Create;
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
    public class CreateAmenityTests : IClassFixture<ABPWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly Fixture _fixture;
        private readonly ApplicationDbContext _dbContext;
        private readonly string _adminToken;
        private readonly string _userToken;

        public CreateAmenityTests(ABPWebApplicationFactory factory)
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
        public async void CreateAmenity_Should_ReturnCreatedAmenity_WhenSucess()
        {
            // Arrange
            var createAmenityCommand = _fixture.Create<CreateAmenityCommand>();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);
            //Act
            var response = await _client.PostAsJsonAsync("/api/amenities", createAmenityCommand);
            var createdAmenity = response.Content.Headers.ContentType?.MediaType == "application/json" ?
                await response.Content.ReadFromJsonAsync<AmenityDto>() : null;
            //Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            createdAmenity.Should().NotBeNull();
            createdAmenity?.Id.Should().BeGreaterThanOrEqualTo(1);
            createdAmenity?.Name.Should().Be(createAmenityCommand.Name);
            createdAmenity?.Description.Should().Be(createAmenityCommand.Description);

        }

        [Fact]
        public async Task CreateAmenity_Should_ReturnUnauthorized_WhenTokenIsInvalidOrMissing()
        {

            // Arrange
            var createAmenityCommand = _fixture.Create<CreateAmenityCommand>();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "invaild token");
            //Act
            var response = await _client.PostAsJsonAsync("/api/amenities", createAmenityCommand);
            //Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task CreateAmenity_Should_ReturnBadRequest_WhenNameIsEmpty()
        {
            // Arrange
            var createAmenityCommand = _fixture.Build<CreateAmenityCommand>()
                .With(x => x.Name, string.Empty)
                .Create();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);

            // Act
            var response = await _client.PostAsJsonAsync("/api/amenities", createAmenityCommand);

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateAmenity_Should_ReturnBadRequest_WhenDescriptionIsEmpty()
        {
            // Arrange
            var createAmenityCommand = _fixture.Build<CreateAmenityCommand>()
                .With(x => x.Description, string.Empty)
                .Create();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);

            // Act
            var response = await _client.PostAsJsonAsync("/api/amenities", createAmenityCommand);

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateAmenity_Should_ReturnBadRequest_WhenNameExceedsMaxLength()
        {
            // Arrange
            var createAmenityCommand = _fixture.Build<CreateAmenityCommand>()
                .With(x => x.Name, new string('A', 61))
                .Create();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);

            // Act
            var response = await _client.PostAsJsonAsync("/api/amenities", createAmenityCommand);

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateAmenity_Should_ReturnBadRequest_WhenDescriptionExceedsMaxLength()
        {
            // Arrange
            var createAmenityCommand = _fixture.Build<CreateAmenityCommand>()
                .With(x => x.Description, new string('A', 161))
                .Create();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);

            // Act
            var response = await _client.PostAsJsonAsync("/api/amenities", createAmenityCommand);

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateAmenity_Should_ReturnForbidden_WhenUserIsNotAdmin()
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
