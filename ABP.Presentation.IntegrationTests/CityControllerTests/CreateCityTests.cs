using ABPIntegrationTests;
using Application.CommandsAndQueries.CityCQ.Commands.Create;
using Application.Dtos.CityDtos;
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

namespace ABP.Presentation.IntegrationTests.CityControllerTests
{
    public class CreateCityTests : IClassFixture<ABPWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly Fixture _fixture;
        private readonly ApplicationDbContext _dbContext;
        private readonly string _adminToken;
        private readonly string _userToken;
        private readonly CreateCityCommand _command;

        public CreateCityTests(ABPWebApplicationFactory factory)
        {
            factory.DatabaseName = $"InMemoryDb_Create";
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
            _command = _fixture.Build<CreateCityCommand>()
                .With(x => x.PostOffice, _fixture.Create<string>()[0..20])
                .With(x => x.Thumbnail, GlobalTestData.GetFormFile())
                .Create();
        }

        [Fact]
        public async void CreateCity_Should_ReturnCreatedCity_WhenSuccess()
        {
            // Arrange

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);
            var content = GlobalTestData.GetMultiPartFormDataFromCommand(_command);
            // Act
            var response = await _client.PostAsync("/api/cities", content);
            var createdCity = response.Content.Headers.ContentType?.MediaType == "application/json" ?
                await response.Content.ReadFromJsonAsync<CityDto>() : null;

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            createdCity.Should().NotBeNull();
            createdCity?.Id.Should().BeGreaterThanOrEqualTo(1);
            createdCity?.Name.Should().Be(_command.Name);
            createdCity?.Country.Should().Be(_command.Country);
            createdCity?.PostOffice.Should().Be(_command.PostOffice);
        }

        [Fact]
        public async Task CreateCity_Should_ReturnUnauthorized_WhenTokenIsInvalidOrMissing()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "invalid token");
            var content = GlobalTestData.GetMultiPartFormDataFromCommand(_command);
            // Act
            var response = await _client.PostAsync("/api/cities", content);

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task CreateCity_Should_ReturnBadRequest_WhenNameIsEmpty()
        {
            // Arrange
            _command.Name = string.Empty;
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);
            var content = GlobalTestData.GetMultiPartFormDataFromCommand(_command);
            // Act
            var response = await _client.PostAsync("/api/cities", content);

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateCity_Should_ReturnBadRequest_WhenNameExceedsMaxLength()
        {
            // Arrange
            _command.Name = new string('A', 51);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);
            var content = GlobalTestData.GetMultiPartFormDataFromCommand(_command);

            // Act
            var response = await _client.PostAsync("/api/cities", content);

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateCity_Should_ReturnBadRequest_WhenThumbnailIsEmpty()
        {
            // Arrange
            _command.Thumbnail = null!;
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);
            var content = GlobalTestData.GetMultiPartFormDataFromCommand(_command);
            // Act
            var response = await _client.PostAsync("/api/cities", content);

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateCity_Should_ReturnBadRequest_WhenThumbnailExtensionInvaild()
        {
            // Arrange
            _command.Thumbnail = GlobalTestData.GetInvaildFormFile();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);
            var content = GlobalTestData.GetMultiPartFormDataFromCommand(_command);
            // Act
            var response = await _client.PostAsync("/api/cities", content);

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateCity_Should_ReturnBadRequest_WhenCountryIsEmpty()
        {
            // Arrange
            _command.Country = string.Empty;
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);
            var content = GlobalTestData.GetMultiPartFormDataFromCommand(_command);
            // Act
            var response = await _client.PostAsync("/api/cities", content);

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateCity_Should_ReturnBadRequest_WhenCountryExceedsMaxLength()
        {
            // Arrange
            _command.Country = new string('A', 51);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);
            var content = GlobalTestData.GetMultiPartFormDataFromCommand(_command);

            // Act
            var response = await _client.PostAsync("/api/cities", content);

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateCity_Should_ReturnBadRequest_WhenPostOfficeIsEmpty()
        {
            // Arrange
            _command.PostOffice = string.Empty;
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);
            var content = GlobalTestData.GetMultiPartFormDataFromCommand(_command);
            // Act
            var response = await _client.PostAsync("/api/cities", content);

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateCity_Should_ReturnBadRequest_WhenPostOfficeExceedsMaxLength()
        {
            // Arrange
            _command.PostOffice = new string('A', 21);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);
            var content = GlobalTestData.GetMultiPartFormDataFromCommand(_command);

            // Act
            var response = await _client.PostAsync("/api/cities", content);

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateCity_Should_ReturnConflict_WhenCityExistsInCountry()
        {
            // Arrange
            var city = _fixture.Build<City>()
                .With(x => x.PostOffice, _fixture.Create<string>()[0..20])
                .Without(x => x.Hotels)
                .Create();
            await _dbContext.Cities.AddAsync(city);
            await _dbContext.SaveChangesAsync();
            _command.Country = city.Country;
            _command.Name = city.Name;
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);
            var content = GlobalTestData.GetMultiPartFormDataFromCommand(_command);

            // Act
            var response = await _client.PostAsync("/api/cities", content);

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }
        [Fact]
        public async Task CreateCity_Should_ReturnConflict_WhenPostOfficeExists() {
            // Arrange
            var city = _fixture.Build<City>()
                .With(x => x.PostOffice, _fixture.Create<string>()[0..20])
                .Without(x => x.Hotels)
                .Create();
            await _dbContext.Cities.AddAsync(city);
            await _dbContext.SaveChangesAsync();
            _command.PostOffice = city.PostOffice;
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);
            var content = GlobalTestData.GetMultiPartFormDataFromCommand(_command);

            // Act
            var response = await _client.PostAsync("/api/cities", content);

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }

        [Fact]
        public async Task CreateCity_Should_ReturnForbidden_WhenUserIsNotAdmin()
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
