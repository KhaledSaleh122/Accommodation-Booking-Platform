using ABPIntegrationTests;
using Application.CommandsAndQueries.HotelCQ.Commands.Create;
using Application.Dtos.HotelDtos;
using AutoFixture;
using Domain.Entities;
using Domain.Enums;
using FluentAssertions;
using Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace ABP.Presentation.IntegrationTests.HotelControllerTests
{
    public class CreateHotelTests : IClassFixture<ABPWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly Fixture _fixture;
        private readonly ApplicationDbContext _dbContext;
        private readonly string _adminToken;
        private readonly string _userToken;
        private readonly CreateHotelCommand _command;

        public CreateHotelTests(ABPWebApplicationFactory factory)
        {
            factory.DatabaseName = Guid.NewGuid().ToString();
            _client = factory.CreateClient();
            _fixture = new Fixture();
            var scope = factory.Services.CreateScope();
            var serviceProvider = scope.ServiceProvider;
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
            JwtTokenHelper jwtTokenHelper = new(configuration, userManager);
            _dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();
            factory.SetupDbContext(_dbContext).GetAwaiter().GetResult();
            _adminToken = jwtTokenHelper.GetJwtTokenAsync("Admin").GetAwaiter().GetResult();
            _userToken = jwtTokenHelper.GetJwtTokenAsync("User").GetAwaiter().GetResult();
            _command = _fixture.Build<CreateHotelCommand>()
                .With(x => x.Thumbnail, GlobalTestData.GetFormFile())
                .With(x => x.Images, GlobalTestData.GetFormFiles(3))
                .With(x => x.HotelType, (int)_fixture.Create<HotelType>())
                .Create();
        }

        [Fact]
        public async void CreateHotel_Should_ReturnCreatedHotel_WhenSuccess()
        {
            // Arrange
            var city = _fixture.Build<City>()
                .Without(x => x.Hotels)
                .With(x => x.Id,_command.CityId)
                .Create();
            await _dbContext.Cities.AddAsync(city);
            await _dbContext.SaveChangesAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);
            var content = GlobalTestData.GetMultiPartFormDataFromCommand(_command);

            // Act
            var response = await _client.PostAsync("/api/v1/hotels", content);
            var createdHotel = response.Content.Headers.ContentType?.MediaType == "application/json" ?
                await response.Content.ReadFromJsonAsync<HotelMinDto>() : null;

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            createdHotel.Should().NotBeNull();
            createdHotel?.Id.Should().BeGreaterThanOrEqualTo(1);
            createdHotel?.Name.Should().Be(_command.Name);
            createdHotel?.Description.Should().Be(_command.Description);
            createdHotel?.Address.Should().Be(_command.Address);
            createdHotel?.HotelType.Should().Be(Enum.GetName(typeof(HotelType),_command.HotelType));
            createdHotel?.Owner.Should().Be(_command.Owner);
            createdHotel?.PricePerNight.Should().Be(_command.PricePerNight);
            createdHotel?.City.Should().Be(city.Name);


        }

        [Fact]
        public async Task CreateHotel_Should_ReturnUnauthorized_WhenTokenIsInvalidOrMissing()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "invalid token");
            var content = GlobalTestData.GetMultiPartFormDataFromCommand(_command);

            // Act
            var response = await _client.PostAsync("/api/v1/hotels", content);

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task CreateHotel_Should_ReturnBadRequest_WhenNameIsEmpty()
        {
            // Arrange
            _command.Name = string.Empty;
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);
            var content = GlobalTestData.GetMultiPartFormDataFromCommand(_command);

            // Act
            var response = await _client.PostAsync("/api/v1/hotels", content);

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateHotel_Should_ReturnBadRequest_WhenNameExceedsMaxLength()
        {
            // Arrange
            _command.Name = new string('A', 51);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);
            var content = GlobalTestData.GetMultiPartFormDataFromCommand(_command);

            // Act
            var response = await _client.PostAsync("/api/v1/hotels", content);

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateHotel_Should_ReturnBadRequest_WhenThumbnailIsEmpty()
        {
            // Arrange
            _command.Thumbnail = null!;
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);
            var content = GlobalTestData.GetMultiPartFormDataFromCommand(_command);

            // Act
            var response = await _client.PostAsync("/api/v1/hotels", content);

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateHotel_Should_ReturnBadRequest_WhenThumbnailExtensionInvalid()
        {
            // Arrange
            _command.Thumbnail = GlobalTestData.GetInvaildFormFile();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);
            var content = GlobalTestData.GetMultiPartFormDataFromCommand(_command);

            // Act
            var response = await _client.PostAsync("/api/v1/hotels", content);

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateHotel_Should_ReturnBadRequest_WhenImagesAreEmpty()
        {
            // Arrange
            _command.Images.Clear();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);
            var content = GlobalTestData.GetMultiPartFormDataFromCommand(_command);

            // Act
            var response = await _client.PostAsync("/api/v1/hotels", content);

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateHotel_Should_ReturnBadRequest_WhenImagesExceedMaxCount()
        {
            // Arrange
            _command.Images = GlobalTestData.GetFormFiles(21);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);
            var content = GlobalTestData.GetMultiPartFormDataFromCommand(_command);

            // Act
            var response = await _client.PostAsync("/api/v1/hotels", content);

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateHotel_Should_ReturnForbidden_WhenUserIsNotAdmin()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _userToken);
            var content = GlobalTestData.GetMultiPartFormDataFromCommand(_command);

            // Act
            var response = await _client.PostAsync("/api/v1/hotels", content);

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task CreateHotel_Should_ReturnBadRequest_WhenCityNotFound()
        {
            // Arrange
            _command.CityId = 0;
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);
            var content = GlobalTestData.GetMultiPartFormDataFromCommand(_command);

            // Act
            var response = await _client.PostAsync("/api/v1/hotels", content);

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateHotel_Should_ReturnBadRequest_WhenPricePerNightIsNegative()
        {
            // Arrange
            _command.PricePerNight = -100;
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);
            var content = GlobalTestData.GetMultiPartFormDataFromCommand(_command);

            // Act
            var response = await _client.PostAsync("/api/v1/hotels", content);

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateHotel_Should_ReturnBadRequest_WhenHotelTypeIsUnknown()
        {
            // Arrange
            _command.HotelType = -1;
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);
            var content = GlobalTestData.GetMultiPartFormDataFromCommand(_command);

            // Act
            var response = await _client.PostAsync("/api/v1/hotels", content);

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
}
