using ABPIntegrationTests;
using Application.CommandsAndQueries.ReviewCQ.Commands.Create;
using AutoFixture;
using Domain.Entities;
using FluentAssertions;
using Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace ABP.Presentation.IntegrationTests.ReviewControllerTests
{
    public class CreateHotelReviewTests : IClassFixture<ABPWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly Fixture _fixture;
        private readonly ApplicationDbContext _dbContext;
        private readonly string _adminToken;
        private readonly string _userToken;

        public CreateHotelReviewTests(ABPWebApplicationFactory factory)
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
        public async Task CreateHotelReview_Should_ReturnCreated_WhenSuccess()
        {
            // Arrange
            var hotel = _fixture.Create<Hotel>();
            await _dbContext.Hotels.AddAsync(hotel);
            await _dbContext.SaveChangesAsync();
            var command = _fixture.Build<CreateReviewCommand>()
                .With(c => c.hotelId, hotel.Id)
                .With(c => c.Rating, new Random().Next(1, 5))
                .Create();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _userToken);

            // Act
            var response = await _client.PostAsJsonAsync($"/api/hotels/{hotel.Id}/reviews", command);
            hotel = await _dbContext.Hotels.Include(o => o.Reviews).Where(x => x.Id == hotel.Id).FirstAsync();
            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            hotel.Reviews.Should().HaveCount(1);
            hotel.Reviews.Should().Contain(x => x.Comment == command.Comment && x.Rating == command.Rating);
        }

        [Fact]
        public async Task CreateHotelReview_Should_ReturnUnauthorized_WhenTokenIsInvalidOrMissing()
        {
            // Arrange
            var command = _fixture.Create<CreateReviewCommand>();
            // Act
            var response = await _client.PostAsJsonAsync("/api/hotels/1/reviews", command);

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task CreateHotelReview_Should_ReturnForbidden_WhenUserIsNotAuthorized()
        {
            // Arrange
            var command = _fixture.Create<CreateReviewCommand>();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);

            // Act
            var response = await _client.PostAsJsonAsync("/api/hotels/1/reviews", command);

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task CreateHotelReview_Should_ReturnNotFound_WhenHotelDoesNotExist()
        {
            // Arrange
            var command = _fixture.Create<CreateReviewCommand>();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _userToken);

            // Act
            var response = await _client.PostAsJsonAsync("/api/hotels/0/reviews", command);

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task CreateHotelReview_Should_ReturnBadRequest_WhenCommandIsNull()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _userToken);

            // Act
            var response = await _client.PostAsync("/api/hotels/1/reviews", null);

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateHotelReview_Should_ReturnConflict_WhenUserAlreadyReviewedHotel()
        {
            // Arrange
            var hotel = _fixture.Create<Hotel>();
            var review = _fixture.Build<Review>()
                .With(r => r.HotelId, hotel.Id)
                .With(r => r.UserId, "UserId")
                .Without(x => x.User)
                .Without(x => x.Hotel)
                .Create();
            await _dbContext.Hotels.AddAsync(hotel);
            await _dbContext.Reviews.AddAsync(review);
            await _dbContext.SaveChangesAsync();
            var command = _fixture.Build<CreateReviewCommand>()
                .With(c => c.hotelId, hotel.Id)
                .With(c => c.Rating, new Random().Next(1, 5))
                .Create();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _userToken);

            // Act
            var response = await _client.PostAsJsonAsync($"/api/hotels/{hotel.Id}/reviews", command);

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }
        [Fact]
        public async Task CreateHotelReview_Should_ReturnBadRequest_WhenRatingOutOfRange()
        {
            // Arrange
            var command = _fixture.Build<CreateReviewCommand>()
                .With(c => c.Rating, -2)
                .Create();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _userToken);

            // Act
            var response = await _client.PostAsJsonAsync($"/api/hotels/1/reviews", command);
            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateHotelReview_Should_ReturnBadRequest_WhenCommentExceedsMaxLength()
        {
            // Arrange
            var command = _fixture.Build<CreateReviewCommand>()
                .With(c => c.Rating, new Random().Next(1, 5))
                .With(c => c.Comment, new string('a', 256))
                .Create();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _userToken);

            // Act
            var response = await _client.PostAsJsonAsync($"/api/hotels/1/reviews", command);
            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
}
