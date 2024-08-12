using ABPIntegrationTests;
using Application.CommandsAndQueries.CityCQ.Commands.Update;
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
    public class UpdateHotelTests : IClassFixture<ABPWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly Fixture _fixture;
        private readonly ApplicationDbContext _dbContext;
        private readonly string _adminToken;
        private readonly string _userToken;
        private readonly UpdateHotelCommand _command;

        public UpdateHotelTests(ABPWebApplicationFactory factory)
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
            _adminToken = jwtTokenHelper.GetJwtTokenAsync("Admin").GetAwaiter().GetResult();
            _userToken = jwtTokenHelper.GetJwtTokenAsync("User").GetAwaiter().GetResult();
            _command = _fixture.Build<UpdateHotelCommand>()
                .With(x => x.HotelType, (int)_fixture.Create<HotelType>())
                .Create();
        }

        [Fact]
        public async Task UpdateHotel_Should_ReturnUpdatedHotel_WhenSuccess()
        {
            // Arrange
            var hotel = _fixture.Build<Hotel>().With(x => x.Id, 1)
                .Without(x => x.Rooms)
                .Without(x => x.Reviews)
                .Without(x => x.RecentlyVisitedHotels)
                .Without(x => x.SpecialOffers)
                .Create();
            _command.hotelId = hotel.Id;
            await _dbContext.Hotels.AddAsync(hotel);
            await _dbContext.SaveChangesAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);

            // Act
            var response = await _client.PatchAsJsonAsync($"/api/v1/hotels/{hotel.Id}", _command);
            var updatedHotel = response.Content.Headers.ContentType?.MediaType == "application/json" ?
                await response.Content.ReadFromJsonAsync<HotelMinDto>() : null;

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            updatedHotel.Should().NotBeNull();
            updatedHotel?.Id.Should().Be(hotel.Id);
            updatedHotel?.Name.Should().Be(_command.Name ?? hotel.Name);
            updatedHotel?.Description.Should().Be(_command.Description ?? hotel.Description);
            updatedHotel?.Address.Should().Be(_command.Address ?? hotel.Address);
            updatedHotel?.Owner.Should().Be(_command.Owner ?? hotel.Owner);
            updatedHotel?.PricePerNight.Should().Be(_command.PricePerNight ?? hotel.PricePerNight);
            updatedHotel?.HotelType.Should().Be(
                _command.HotelType is null ? hotel.HotelType.ToString() : Enum.GetName(typeof(HotelType), _command.HotelType)
            );
        }
        [Fact]
        public async Task UpdateHotel_Should_UpdateOnlyName_WhenOtherPropertiesIsEmpty()
        {
            //Arrange
            var hotel = _fixture.Build<Hotel>().With(x => x.Id, 2)
                .Without(x => x.Rooms)
                .Without(x => x.Reviews)
                .Without(x => x.RecentlyVisitedHotels)
                .Without(x => x.SpecialOffers)
                .Create();
            var command = new UpdateHotelCommand
            {
                Name = _fixture.Create<string>(),
                hotelId = hotel.Id
            };
            await _dbContext.Hotels.AddAsync(hotel);
            await _dbContext.SaveChangesAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);
            // Act
            var response = await _client.PatchAsJsonAsync($"/api/v1/hotels/{hotel.Id}", command);
            var updatedHotel = response.Content.Headers.ContentType?.MediaType == "application/json" ?
                await response.Content.ReadFromJsonAsync<HotelMinDto>() : null;
            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            updatedHotel.Should().NotBeNull();
            updatedHotel?.Id.Should().Be(hotel.Id);
            updatedHotel?.Name.Should().Be(command.Name);
            updatedHotel?.Description.Should().Be(hotel.Description);
            updatedHotel?.Address.Should().Be(hotel.Address);
            updatedHotel?.Owner.Should().Be(hotel.Owner);
            updatedHotel?.PricePerNight.Should().Be(hotel.PricePerNight);
            updatedHotel?.HotelType.Should().Be(hotel.HotelType.ToString());
        }

        [Fact]
        public async Task UpdateHotel_Should_ReturnUnauthorized_WhenTokenIsInvalidOrMissing()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "invalid token");
            var content = JsonContent.Create(_command);

            // Act
            var response = await _client.PatchAsync($"/api/v1/hotels/1", content);

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task UpdateHotel_Should_ReturnNotFound_WhenHotelIdIsInvalid()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);
            // Act
            var response = await _client.PatchAsJsonAsync($"/api/v1/hotels/0", _command);

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task UpdateHotel_Should_ReturnBadRequest_WhenNameExceedsMaxLength()
        {
            // Arrange
            _command.Name = new string('A', 51);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);

            // Act
            var response = await _client.PatchAsJsonAsync($"/api/v1/hotels/1", _command);

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task UpdateHotel_Should_ReturnBadRequest_WhenPricePerNightIsNegative()
        {
            // Arrange
            _command.PricePerNight = -100;
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);

            // Act
            var response = await _client.PatchAsJsonAsync($"/api/v1/hotels/1", _command);

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task UpdateHotel_Should_ReturnBadRequest_WhenHotelTypeIsUnknown()
        {
            // Arrange
            _command.HotelType = -1;
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);

            // Act
            var response = await _client.PatchAsJsonAsync($"/api/v1/hotels/1", _command);

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task UpdateHotel_Should_ReturnForbidden_WhenUserIsNotAdmin()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _userToken);
            var content = JsonContent.Create(_command);

            // Act
            var response = await _client.PatchAsJsonAsync($"/api/v1/hotels/1", _command);

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task UpdateHotel_Should_ReturnSucess_WhenCommandIsNull()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);

            // Act
            var response = await _client.PatchAsync($"/api/v1/hotels/1", null);

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task UpdateHotel_Should_ReturnBadRequest_WhenAddressIsTooLong()
        {
            // Arrange
            _command.Address = new string('A', 101); // Address exceeds maximum length
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);

            // Act
            var response = await _client.PatchAsJsonAsync($"/api/v1/hotels/1", _command);

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task UpdateHotel_Should_ReturnBadRequest_WhenDescriptionIsTooLong()
        {
            // Arrange
            _command.Description = new string('A', 161); // Description exceeds maximum length
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);

            // Act
            var response = await _client.PatchAsJsonAsync($"/api/v1/hotels/1", _command);

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task UpdateHotel_Should_ReturnBadRequest_WhenOwnerIsTooLong()
        {
            // Arrange
            _command.Owner = new string('A', 51);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);

            // Act
            var response = await _client.PatchAsJsonAsync($"/api/v1/hotels/1", _command);

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
}
