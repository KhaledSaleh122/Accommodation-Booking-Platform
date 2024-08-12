using ABPIntegrationTests;
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
using System.Net.Http.Json;

namespace ABP.Presentation.IntegrationTests.SpecialOfferControllerTests
{
    public class GetTopSpecialFeatureDealOffersTests : IClassFixture<ABPWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly Fixture _fixture;
        private readonly ApplicationDbContext _dbContext;

        public GetTopSpecialFeatureDealOffersTests(ABPWebApplicationFactory factory)
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
        }

        [Fact]
        public async Task GetTopSpecialFeatureDealOffers_Should_ReturnOk_WhenSuccess()
        {
            // Arrange
            var hotel = _fixture.Create<Hotel>();
            var specialOffers = new List<SpecialOffer>();
            var specialOffersFeaturedDeals = _fixture.Build<SpecialOffer>()
                .With(so => so.HotelId, hotel.Id)
                .Without(so => so.Hotel)
                .With(so => so.OfferType, OfferType.FeatureDeal)
                .With(so => so.DiscountPercentage, new Random().Next(1, 100))
                .With(so => so.ExpireDate, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)))
                .CreateMany(10)
                .ToList();
            specialOffers.AddRange(specialOffersFeaturedDeals);
            var specialOffersGeneral = _fixture.Build<SpecialOffer>()
                .With(so => so.HotelId, hotel.Id)
                .Without(so => so.Hotel)
                .With(so => so.DiscountPercentage, new Random().Next(1, 100))
                .With(so => so.ExpireDate, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)))
                .CreateMany(10)
                .ToList();
            specialOffers.AddRange(specialOffersGeneral);
            await _dbContext.Hotels.AddAsync(hotel);
            await _dbContext.SpecialOffers.AddRangeAsync(specialOffers);
            await _dbContext.SaveChangesAsync();
            var topSpecialOffers = specialOffers.Where(sp => sp.OfferType == OfferType.FeatureDeal)
                .OrderByDescending(sp => sp.DiscountPercentage).ToList();
            // Act
            var response = await _client.GetAsync("/api/v1/special-offers");
            var featuredDeals = await response.Content.ReadFromJsonAsync<IEnumerable<FeaturedDealsDto>>();

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            featuredDeals.Should().NotBeNull();
            featuredDeals.Should().HaveCount(5);
            featuredDeals.Should().Contain(x => topSpecialOffers.Any(y => y.Id == x.Id));
        }

        [Fact]
        public async Task GetTopSpecialFeatureDealOffers_Should_ReturnEmpty_WhenNoFeatureDealsExist()
        {
            // Act
            var response = await _client.GetAsync("/api/v1/special-offers");

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadFromJsonAsync<IEnumerable<FeaturedDealsDto>>();
            content.Should().NotBeNull();
            content.Should().BeEmpty();
        }
    }
}
