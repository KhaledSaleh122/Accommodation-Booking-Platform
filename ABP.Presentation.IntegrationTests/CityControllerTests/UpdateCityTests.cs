using ABPIntegrationTests;
using Application.CommandsAndQueries.CityCQ.Commands.Update;
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
    public class UpdateCityTests : IClassFixture<ABPWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly Fixture _fixture;
        private readonly ApplicationDbContext _dbContext;
        private readonly string _adminToken;
        private readonly string _userToken;
        private readonly UpdateCityCommand _command;

        public UpdateCityTests(ABPWebApplicationFactory factory)
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
            _command = _fixture.Build<UpdateCityCommand>()
                .With(x => x.PostOffice, _fixture.Create<string>()[0..20])
                .With(x => x.Thumbnail, GlobalTestData.GetFormFile())
                .Create();
        }

        [Fact]
        public async Task UpdateCity_Should_ReturnUpdatedCity_WhenSuccessful()
        {
            // Arrange
            var city = _fixture.Build<City>()
                .With(x => x.PostOffice, _fixture.Create<string>()[0..20])
                .Without(x => x.Hotels)
                .Create();
            await _dbContext.Cities.AddAsync(city);
            await _dbContext.SaveChangesAsync();
            _command.id = city.Id;
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);
            var content = GlobalTestData.GetMultiPartFormDataFromCommand(_command);

            // Act
            var response = await _client.PutAsync($"/api/cities/{city.Id}", content);
            var updatedCity = response.Content.Headers.ContentType?.MediaType == "application/json" ?
                await response.Content.ReadFromJsonAsync<CityDto>() : null;

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            updatedCity.Should().NotBeNull();
            updatedCity?.Id.Should().Be(city.Id);
            updatedCity?.PostOffice.Should().Be(_command.PostOffice);
            updatedCity?.Country.Should().Be(_command.Country);
            updatedCity?.Name.Should().Be(_command.Name);
        }

        [Fact]
        public async Task UpdateCity_Should_ReturnNotFound_WhenCityNotFound()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);
            var content = GlobalTestData.GetMultiPartFormDataFromCommand(_command);

            // Act
            var response = await _client.PutAsync($"/api/cities/0", content);

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task UpdateCity_Should_ReturnUnauthorized_WhenTokenIsInvalidOrMissing()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "invalid token");
            var content = GlobalTestData.GetMultiPartFormDataFromCommand(_command);

            // Act
            var response = await _client.PutAsync($"/api/cities/1", content);

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task UpdateCity_Should_ReturnBadRequest_WhenNameIsEmpty()
        {
            // Arrange
            _command.Name = string.Empty;
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);
            var content = GlobalTestData.GetMultiPartFormDataFromCommand(_command);

            // Act
            var response = await _client.PutAsync($"/api/cities/1", content);

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task UpdateCity_Should_ReturnBadRequest_WhenNameExceedsMaxLength()
        {
            // Arrange
            _command.Name = new string('A', 51);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);
            var content = GlobalTestData.GetMultiPartFormDataFromCommand(_command);

            // Act
            var response = await _client.PutAsync($"/api/cities/1", content);

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task UpdateCity_Should_ReturnBadRequest_WhenCountryIsEmpty()
        {
            // Arrange
            _command.Country = string.Empty;
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);
            var content = GlobalTestData.GetMultiPartFormDataFromCommand(_command);

            // Act
            var response = await _client.PutAsync($"/api/cities/1", content);

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task UpdateCity_Should_ReturnBadRequest_WhenCountryExceedsMaxLength()
        {
            // Arrange
            _command.Country = new string('A', 51);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);
            var content = GlobalTestData.GetMultiPartFormDataFromCommand(_command);

            // Act
            var response = await _client.PutAsync($"/api/cities/1", content);

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task UpdateCity_Should_ReturnBadRequest_WhenPostOfficeIsEmpty()
        {
            // Arrange
            _command.PostOffice = string.Empty;
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);
            var content = GlobalTestData.GetMultiPartFormDataFromCommand(_command);

            // Act
            var response = await _client.PutAsync($"/api/cities/1", content);

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task UpdateCity_Should_ReturnBadRequest_WhenPostOfficeExceedsMaxLength()
        {
            // Arrange
            _command.PostOffice = new string('A', 21);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);
            var content = GlobalTestData.GetMultiPartFormDataFromCommand(_command);

            // Act
            var response = await _client.PutAsync($"/api/cities/1", content);

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task UpdateCity_Should_ReturnBadRequest_WhenThumbnailIsEmpty()
        {
            // Arrange
            _command.Thumbnail = null!;
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);
            var content = GlobalTestData.GetMultiPartFormDataFromCommand(_command);

            // Act
            var response = await _client.PutAsync($"/api/cities/1", content);

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task UpdateCity_Should_ReturnBadRequest_WhenThumbnailExtensionIsInvalid()
        {
            // Arrange
            _command.Thumbnail = GlobalTestData.GetInvaildFormFile();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);
            var content = GlobalTestData.GetMultiPartFormDataFromCommand(_command);

            // Act
            var response = await _client.PutAsync($"/api/cities/1", content);

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task UpdateCity_Should_ReturnConflict_WhenCityExistsInCountry()
        {
            // Arrange
            var city = _fixture.Build<City>()
                .With(x => x.PostOffice, _fixture.Create<string>()[0..20])
                .Without(x => x.Hotels)
                .Create();
            await _dbContext.Cities.AddAsync(city);
            await _dbContext.SaveChangesAsync();
            _command.id = city.Id;
            _command.Name = city.Name;
            _command.Country = city.Country;
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);
            var content = GlobalTestData.GetMultiPartFormDataFromCommand(_command);

            // Act
            var response = await _client.PutAsync($"/api/cities/{city.Id}", content);

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }

        [Fact]
        public async Task UpdateCity_Should_ReturnConflict_WhenPostOfficeExists()
        {
            // Arrange
            var city = _fixture.Build<City>()
                .With(x => x.PostOffice, _fixture.Create<string>()[0..20])
                .Without(x => x.Hotels)
                .Create();
            await _dbContext.Cities.AddAsync(city);
            await _dbContext.SaveChangesAsync();
            _command.id = city.Id;
            _command.PostOffice = city.PostOffice;
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);
            var content = GlobalTestData.GetMultiPartFormDataFromCommand(_command);

            // Act
            var response = await _client.PutAsync($"/api/cities/{city.Id}", content);

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }

        [Fact]
        public async Task UpdateCity_Should_ReturnForbidden_WhenUserIsNotAdmin()
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
