using ABPIntegrationTests;
using Application.CommandsAndQueries.SpecialOfferCQ.Commands.Create;
using Application.Dtos.SpecialOfferDtos;
using AutoFixture;
using Domain.Entities;
using Domain.Enums;
using FluentAssertions;
using Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace ABP.Presentation.IntegrationTests.SpecialOfferControllerTests
{
    public class CreateSpecialOfferTests : IClassFixture<ABPWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly Fixture _fixture;
        private readonly ApplicationDbContext _dbContext;
        private readonly string _adminToken;
        private readonly string _userToken;

        public CreateSpecialOfferTests(ABPWebApplicationFactory factory)
        {
            factory.DatabaseName = Guid.NewGuid().ToString();
            _client = factory.CreateClient();
            _fixture = new Fixture();
            var scope = factory.Services.CreateScope();
            var serviceProvider = scope.ServiceProvider;
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            _fixture.Customize<DateOnly>(composer => composer.FromFactory<int, int, int>((year, month, day) =>
            {
                var now = DateTime.Now;
                return new DateOnly(now.Year, now.Month, now.Day);
            }));
            _fixture.Customize<Hotel>(x => x
                .Without(h => h.Reviews)
                .Without(h => h.RecentlyVisitedHotels)
                .Without(h => h.Rooms)
                .Without(h => h.SpecialOffers)
            );
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
            JwtTokenHelper jwtTokenHelper = new(configuration, userManager);
            _dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();
            factory.SetupDbContext(_dbContext).GetAwaiter().GetResult();
            _adminToken = jwtTokenHelper.GetJwtTokenAsync("Admin").GetAwaiter().GetResult();
            _userToken = jwtTokenHelper.GetJwtTokenAsync("User").GetAwaiter().GetResult();
        }

        [Fact]
        public async Task CreateSpecialOffer_Should_ReturnCreated_WhenSuccess()
        {
            // Arrange
            var hotel = _fixture.Create<Hotel>();
            await _dbContext.Hotels.AddAsync(hotel);
            await _dbContext.SaveChangesAsync();
            var command = _fixture.Build<CreateSpecialOfferCommand>()
                .With(c => c.hotelId, hotel.Id)
                .With(c => c.DiscountPercentage, new Random().Next(1, 100))
                .With(c => c.ExpireDate, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2)))
                .Create();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);

            // Act
            var response = await _client.PostAsJsonAsync($"/api/hotels/{hotel.Id}/special-offers", command);
            var specialOffer = await response.Content.ReadFromJsonAsync<SpecialOfferDto>();

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            specialOffer.Should().NotBeNull();
            specialOffer?.OfferType.Should().Be(command.OfferType);
            specialOffer?.DiscountPercentage.Should().Be(command.DiscountPercentage);
            specialOffer?.ExpireDate.Should().Be(command.ExpireDate);
        }

        [Fact]
        public async Task CreateSpecialOffer_Should_ReturnUnauthorized_WhenTokenIsInvalidOrMissing()
        {
            // Arrange
            var command = _fixture.Create<CreateSpecialOfferCommand>();

            // Act
            var response = await _client.PostAsJsonAsync("/api/hotels/1/special-offers", command);

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task CreateSpecialOffer_Should_ReturnForbidden_WhenUserIsNotAuthorized()
        {
            // Arrange
            var command = _fixture.Create<CreateSpecialOfferCommand>();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _userToken);

            // Act
            var response = await _client.PostAsJsonAsync("/api/hotels/1/special-offers", command);

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task CreateSpecialOffer_Should_ReturnNotFound_WhenHotelDoesNotExist()
        {
            // Arrange
            var command = _fixture.Create<CreateSpecialOfferCommand>();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);

            // Act
            var response = await _client.PostAsJsonAsync("/api/hotels/0/special-offers", command);

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task CreateSpecialOffer_Should_ReturnBadRequest_WhenCommandIsNull()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);

            // Act
            var response = await _client.PostAsync("/api/hotels/1/special-offers", null);

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateSpecialOffer_Should_ReturnBadRequest_WhenDiscountPercentageOutOfRange()
        {
            // Arrange
            var command = _fixture.Build<CreateSpecialOfferCommand>()
                .With(c => c.DiscountPercentage, 150)
                .With(c => c.ExpireDate, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2)))
                .Create();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);

            // Act
            var response = await _client.PostAsJsonAsync($"/api/hotels/1/special-offers", command);

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateSpecialOffer_Should_ReturnBadRequest_WhenExpireDateIsInvalid()
        {
            // Arrange
            var command = _fixture.Build<CreateSpecialOfferCommand>()
                .With(c => c.DiscountPercentage, 60)
                .With(c => c.ExpireDate, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)))
                .Create();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);

            // Act
            var response = await _client.PostAsJsonAsync($"/api/hotels/1/special-offers", command);

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateSpecialOffer_Should_ReturnBadRequest_WhenOfferTypeIsInvalid()
        {
            // Arrange
            var command = _fixture.Build<CreateSpecialOfferCommand>()
                .With(c => c.DiscountPercentage, new Random().Next(1, 100))
                .With(c => c.ExpireDate, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2)))
                .With(c => c.OfferType, (OfferType)999)
                .Create();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);

            // Act
            var response = await _client.PostAsJsonAsync($"/api/hotels/1/special-offers", command);

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateSpecialOffer_Should_ReturnConflict_WhenSpecialOfferIdAlreadyExists()
        {
            // Arrange
            var hotel = _fixture.Create<Hotel>();
            var specialOffer = _fixture.Build<SpecialOffer>()
                .With(so => so.HotelId, hotel.Id)
                .Create();
            await _dbContext.Hotels.AddAsync(hotel);
            await _dbContext.SpecialOffers.AddAsync(specialOffer);
            await _dbContext.SaveChangesAsync();
            var command = _fixture.Build<CreateSpecialOfferCommand>()
                .With(c => c.hotelId, hotel.Id)
                .With(c => c.Id, specialOffer.Id)
                .With(c => c.DiscountPercentage, new Random().Next(1, 100))
                .With(c => c.ExpireDate, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2)))
                .Create();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);

            // Act
            var response = await _client.PostAsJsonAsync($"/api/hotels/{hotel.Id}/special-offers", command);

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }
    }
}
