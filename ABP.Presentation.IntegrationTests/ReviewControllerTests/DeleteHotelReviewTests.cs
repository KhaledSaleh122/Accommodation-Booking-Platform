using ABPIntegrationTests;
using Application.Dtos.ReviewDtos;
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

namespace ABP.Presentation.IntegrationTests.ReviewControllerTests
{
    public class DeleteHotelReviewTests : IClassFixture<ABPWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly Fixture _fixture;
        private readonly ApplicationDbContext _dbContext;
        private readonly string _adminToken;
        private readonly string _userToken;

        public DeleteHotelReviewTests(ABPWebApplicationFactory factory)
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
        public async Task DeleteHotelReview_Should_ReturnOk_WhenSuccess()
        {
            // Arrange
            var hotel = _fixture.Create<Hotel>();
            var review = _fixture.Build<Review>()
                .With(r => r.HotelId, hotel.Id)
                .With(r => r.UserId, "UserId")
                .With(c => c.Rating, new Random().Next(1, 5))
                .Without(c => c.Hotel)
                .Without(c => c.User)
                .Create();

            await _dbContext.Hotels.AddAsync(hotel);
            await _dbContext.Reviews.AddAsync(review);
            await _dbContext.SaveChangesAsync();


            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);

            // Act
            var response = await _client.DeleteAsync($"/api/v1/hotels/{hotel.Id}/reviews/{"UserId"}");
            var deletedReview = await response.Content.ReadFromJsonAsync<ReviewDto>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            deletedReview.Should().NotBeNull();
            deletedReview?.Comment.Should().Be(review.Comment);
            deletedReview?.Rating.Should().Be(review.Rating);
        }

        [Fact]
        public async Task DeleteHotelReview_Should_ReturnOk_WhenUserIsOwner()
        {
            // Arrange
            var hotel = _fixture.Create<Hotel>();
            var review = _fixture.Build<Review>()
                .With(r => r.HotelId, hotel.Id)
                .With(r => r.UserId, "UserId")
                .With(c => c.Rating, new Random().Next(1, 5))
                .Without(c => c.Hotel)
                .Without(c => c.User)
                .Create();

            await _dbContext.Hotels.AddAsync(hotel);
            await _dbContext.Reviews.AddAsync(review);
            await _dbContext.SaveChangesAsync();


            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _userToken);

            // Act
            var response = await _client.DeleteAsync($"/api/v1/hotels/{hotel.Id}/reviews/{"UserId"}");
            var deletedReview = await response.Content.ReadFromJsonAsync<ReviewDto>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            deletedReview.Should().NotBeNull();
            deletedReview?.Comment.Should().Be(review.Comment);
            deletedReview?.Rating.Should().Be(review.Rating);
        }


        [Fact]
        public async Task DeleteHotelReview_Should_ReturnNotFound_WhenReviewDoesNotExist()
        {
            // Arrange
            var hotel = _fixture.Create<Hotel>();

            await _dbContext.Hotels.AddAsync(hotel);
            await _dbContext.SaveChangesAsync();

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);

            // Act
            var response = await _client.DeleteAsync($"/api/v1/hotels/{hotel.Id}/reviews/{"UserId"}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task DeleteHotelReview_Should_ReturnUnauthorized_WhenTokenIsInvalidOrMissing()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "invalid token");

            // Act
            var response = await _client.DeleteAsync($"/api/v1/hotels/1/reviews/userId");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task DeleteHotelReview_Should_ReturnForbidden_WhenUserIsNotAdminOrOwner()
        {
            // Arrange
            var hotel = _fixture.Create<Hotel>();
            var review = _fixture.Build<Review>()
                .With(r => r.HotelId, hotel.Id)
                .With(r => r.UserId, "User1Id")
                .Create();

            await _dbContext.Hotels.AddAsync(hotel);
            await _dbContext.Reviews.AddAsync(review);
            await _dbContext.SaveChangesAsync();

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _userToken);

            // Act
            var response = await _client.DeleteAsync($"/api/v1/hotels/{hotel.Id}/reviews/{"User1Id"}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }
    }
}
