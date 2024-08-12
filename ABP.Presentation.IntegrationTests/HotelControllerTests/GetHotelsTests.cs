using ABPIntegrationTests;
using Application.Dtos.HotelDtos;
using AutoFixture;
using Domain.Entities;
using Domain.Enums;
using FluentAssertions;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Presentation.Responses.Pagination;
using System.Diagnostics.Metrics;
using System.Net;
using System.Net.Http.Json;

namespace ABP.Presentation.IntegrationTests.HotelControllerTests
{
    public class GetHotelsTests : IClassFixture<ABPWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly Fixture _fixture;
        private readonly ApplicationDbContext _dbContext;

        public GetHotelsTests(ABPWebApplicationFactory factory)
        {
            factory.DatabaseName = Guid.NewGuid().ToString();
            _client = factory.CreateClient();
            _fixture = new Fixture();
            var scope = factory.Services.CreateScope();
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            _fixture.Customize<DateOnly>(composer => composer.FromFactory<int, int, int>((year, month, day) =>
            {
                var now = DateTime.Now;
                return new DateOnly(now.Year, now.Month, now.Day);
            }));
            _fixture.Customize<Room>(x => x
                .Without(x => x.BookingRooms)
                .Without(x => x.Images)
                .Without(x => x.HotelId)
            );
            _fixture.Customize<Hotel>(x => x
                .Without(x => x.HotelAmenity)
                .Without(x => x.Reviews)
                .Without(x => x.RecentlyVisitedHotels)
                .Without(x => x.SpecialOffers)
            );
            _dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            factory.SetupDbContext(_dbContext).GetAwaiter().GetResult();
        }

        [Fact]
        public async Task GetHotels_Should_ReturnPaginatedHotels_WhenSuccess()
        {
            // Arrange
            var hotels = _fixture.CreateMany<Hotel>(5);
            await _dbContext.Hotels.AddRangeAsync(hotels);
            await _dbContext.SaveChangesAsync();

            // Act
            var response = await _client.GetAsync("/api/v1/hotels?page=1&pageSize=10");
            var result = response.Content.Headers.ContentType?.MediaType == "application/json" ?
                await response.Content.ReadFromJsonAsync<ResultWithPaginationResponse<IEnumerable<HotelDto>>>() : null;

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            result.Should().NotBeNull();
            result?.Results.Should().HaveCount(5);
            result?.TotalRecords.Should().Be(5);
            result?.Page.Should().Be(1);
            result?.PageSize.Should().Be(10);
        }

        [Fact]
        public async Task GetHotels_Should_ReturnEmptyList_WhenNoHotelsExist()
        {
            // Act
            var response = await _client.GetAsync("/api/v1/hotels?page=1&pageSize=10");
            var result = response.Content.Headers.ContentType?.MediaType == "application/json" ?
                await response.Content.ReadFromJsonAsync<ResultWithPaginationResponse<IEnumerable<HotelDto>>>() : null;

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            result.Should().NotBeNull();
            result?.Results.Should().BeEmpty();
            result?.TotalRecords.Should().Be(0);
            result?.Page.Should().Be(1);
            result?.PageSize.Should().Be(10);
        }

        [Fact]
        public async Task GetHotels_Should_ReturnEmptyList_WhenPageIsOutOfRange()
        {
            // Arrange
            var hotels = _fixture.CreateMany<Hotel>(5);
            await _dbContext.Hotels.AddRangeAsync(hotels);
            await _dbContext.SaveChangesAsync();

            // Act
            var response = await _client.GetAsync("/api/v1/hotels?page=2&pageSize=10");
            var result = response.Content.Headers.ContentType?.MediaType == "application/json" ?
                await response.Content.ReadFromJsonAsync<ResultWithPaginationResponse<IEnumerable<HotelDto>>>() : null;

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            result.Should().NotBeNull();
            result?.Results.Should().BeEmpty();
            result?.TotalRecords.Should().Be(5);
            result?.Page.Should().Be(2);
            result?.PageSize.Should().Be(10);
        }

        [Fact]
        public async Task GetHotels_Should_ReturnHotels_WithDifferentPageSizes()
        {
            // Arrange
            var hotels = _fixture.CreateMany<Hotel>(15);
            await _dbContext.Hotels.AddRangeAsync(hotels);
            await _dbContext.SaveChangesAsync();

            // Act & Assert for PageSize=5
            var responsePageSize5 = await _client.GetAsync("/api/v1/hotels?page=1&pageSize=5");
            var resultPageSize5 = responsePageSize5.Content.Headers.ContentType?.MediaType == "application/json" ?
                await responsePageSize5.Content.ReadFromJsonAsync<ResultWithPaginationResponse<IEnumerable<HotelDto>>>() : null;
            responsePageSize5.StatusCode.Should().Be(HttpStatusCode.OK);
            resultPageSize5.Should().NotBeNull();
            resultPageSize5?.Results.Should().HaveCount(5);
            resultPageSize5?.TotalRecords.Should().Be(15);
            resultPageSize5?.Page.Should().Be(1);
            resultPageSize5?.PageSize.Should().Be(5);

            // Act & Assert for PageSize=10
            var responsePageSize10 = await _client.GetAsync("/api/v1/hotels?page=1&pageSize=10");
            var resultPageSize10 = responsePageSize10.Content.Headers.ContentType?.MediaType == "application/json" ?
                await responsePageSize10.Content.ReadFromJsonAsync<ResultWithPaginationResponse<IEnumerable<HotelDto>>>() : null;
            responsePageSize10.StatusCode.Should().Be(HttpStatusCode.OK);
            resultPageSize10.Should().NotBeNull();
            resultPageSize10?.Results.Should().HaveCount(10);
            resultPageSize10?.TotalRecords.Should().Be(15);
            resultPageSize10?.Page.Should().Be(1);
            resultPageSize10?.PageSize.Should().Be(10);
        }

        [Fact]
        public async Task GetHotels_Should_ReturnHotels_WhenInvalidPageParameters()
        {
            // Arrange
            var hotels = _fixture.CreateMany<Hotel>(15);
            await _dbContext.Hotels.AddRangeAsync(hotels);
            await _dbContext.SaveChangesAsync();

            // Act
            var response = await _client.GetAsync("/api/v1/hotels?page=-1&pageSize=0");
            var result = response.Content.Headers.ContentType?.MediaType == "application/json" ?
                await response.Content.ReadFromJsonAsync<ResultWithPaginationResponse<IEnumerable<HotelDto>>>() : null;

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            result.Should().NotBeNull();
            result?.Results.Should().HaveCount(10);
            result?.TotalRecords.Should().Be(15);
            result?.Page.Should().Be(1);
            result?.PageSize.Should().Be(10);
        }

        [Fact]
        public async Task GetHotels_Should_HandleLargeNumberOfHotels()
        {
            // Arrange
            var hotels = _fixture.CreateMany<Hotel>(1000);
            await _dbContext.Hotels.AddRangeAsync(hotels);
            await _dbContext.SaveChangesAsync();

            // Act
            var response = await _client.GetAsync("/api/v1/hotels?page=10&pageSize=100");
            var result = response.Content.Headers.ContentType?.MediaType == "application/json" ?
                await response.Content.ReadFromJsonAsync<ResultWithPaginationResponse<IEnumerable<HotelDto>>>() : null;

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            result.Should().NotBeNull();
            result?.Results.Should().HaveCount(100);
            result?.TotalRecords.Should().Be(1000);
            result?.Page.Should().Be(10);
            result?.PageSize.Should().Be(100);
        }

        [Fact]
        public async Task GetHotels_Should_ReturnHotelsFilteredByCity()
        {
            // Arrange
            var city = _fixture.Build<City>()
                .With(x => x.Name, "New York")
                .Without(x => x.Hotels)
                .Create();
            var otherHotels = _fixture.CreateMany<Hotel>(3);
            var hotels = _fixture.CreateMany<Hotel>(5);
            foreach (var item in hotels)
            {
                item.City = city;
            }
            await _dbContext.Hotels.AddRangeAsync(hotels.Concat(otherHotels));
            await _dbContext.SaveChangesAsync();

            // Act
            var response = await _client.GetAsync("/api/v1/hotels?page=1&pageSize=10&city=New York");
            var result = response.Content.Headers.ContentType?.MediaType == "application/json" ?
                await response.Content.ReadFromJsonAsync<ResultWithPaginationResponse<IEnumerable<HotelDto>>>() : null;

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            result.Should().NotBeNull();
            result?.Results.Should().HaveCount(5);
            result?.TotalRecords.Should().Be(5);
            result?.Page.Should().Be(1);
            result?.PageSize.Should().Be(10);
            result?.Results.All(h => h.City == "New York").Should().BeTrue();
        }

        [Fact]
        public async Task GetHotels_Should_ReturnHotelsFilteredByCountry()
        {
            // Arrange
            var city = _fixture.Build<City>()
                .With(x => x.Country, "USA")
                .Without(x => x.Hotels)
                .Create();
            var otherHotels = _fixture.CreateMany<Hotel>(3);
            var hotels = _fixture.CreateMany<Hotel>(5);
            foreach (var item in hotels)
            {
                item.City = city;
            }
            await _dbContext.Hotels.AddRangeAsync(hotels.Concat(otherHotels));
            await _dbContext.SaveChangesAsync();

            // Act
            var response = await _client.GetAsync("/api/v1/hotels?page=1&pageSize=10&country=USA");
            var result = response.Content.Headers.ContentType?.MediaType == "application/json" ?
                await response.Content.ReadFromJsonAsync<ResultWithPaginationResponse<IEnumerable<HotelDto>>>() : null;

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            result.Should().NotBeNull();
            result?.Results.Should().HaveCount(5);
            result?.TotalRecords.Should().Be(5);
            result?.Page.Should().Be(1);
            result?.PageSize.Should().Be(10);
            result?.Results.All(h => h.Country == "USA").Should().BeTrue();
        }

        [Fact]
        public async Task GetHotels_Should_ReturnHotelsFilteredByAmenities()
        {
            // Arrange
            var amenity = _fixture.Build<Amenity>()
                .With(a => a.Id, 10)
                .With(a => a.Name, "Free WiFi")
                .Without(a => a.HotelAmenity)
                .Create();
            var otherHotels = _fixture.CreateMany<Hotel>(3);
            var hotels = _fixture.CreateMany<Hotel>(3);
            foreach (var item in hotels)
            {
                item.HotelAmenity = [ new HotelAmenity { AmenityId = 10, HotelId = item.Id }];
            }
            await _dbContext.Amenities.AddAsync(amenity);
            await _dbContext.Hotels.AddRangeAsync(hotels.Concat(otherHotels));
            await _dbContext.SaveChangesAsync();
            // Act
            var response = await _client.GetAsync("/api/v1/hotels?page=1&pageSize=10&amenities=10");
            var result = response.Content.Headers.ContentType?.MediaType == "application/json" ?
                await response.Content.ReadFromJsonAsync<ResultWithPaginationResponse<IEnumerable<HotelDto>>>() : null;

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            result.Should().NotBeNull();
            result?.Results.Should().HaveCount(3);
            result?.TotalRecords.Should().Be(3);
            result?.Page.Should().Be(1);
            result?.PageSize.Should().Be(10);
            result?.Results.All(h => h.Amenities.Any(ha => ha.Name == "Free WiFi")).Should().BeTrue();
        }

        [Fact]
        public async Task GetHotels_Should_ReturnHotelsFilteredByHotelType()
        {
            // Arrange
            var otherHotels = _fixture.CreateMany<Hotel>(3);
            foreach (var item in otherHotels)
            {
                item.HotelType = HotelType.Boutique;
            }
            var hotels = _fixture.CreateMany<Hotel>(4);
            foreach (var item in hotels)
            {
                item.HotelType = HotelType.Luxury;
            }
            await _dbContext.Hotels.AddRangeAsync(hotels.Concat(otherHotels));
            await _dbContext.SaveChangesAsync();

            // Act
            var response = await _client.GetAsync("/api/v1/hotels?page=1&pageSize=10&hotelType=Luxury");
            var result = response.Content.Headers.ContentType?.MediaType == "application/json" ?
                await response.Content.ReadFromJsonAsync<ResultWithPaginationResponse<IEnumerable<HotelDto>>>() : null;

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            result.Should().NotBeNull();
            result?.Results.Should().HaveCount(4);
            result?.TotalRecords.Should().Be(4);
            result?.Page.Should().Be(1);
            result?.PageSize.Should().Be(10);
            result?.Results.All(h => h.HotelType == HotelType.Luxury.ToString()).Should().BeTrue();
        }

        [Fact]
        public async Task GetHotels_Should_ReturnHotelsFilteredByPriceRange()
        {
            // Arrange
            var expensiveHotels = _fixture.CreateMany<Hotel>(2);
            foreach (var item in expensiveHotels)
            {
                item.PricePerNight = 500;
            }
            var hotels = _fixture.CreateMany<Hotel>(3);
            foreach (var item in hotels)
            {
                item.PricePerNight = 150;
            }
            await _dbContext.Hotels.AddRangeAsync(hotels.Concat(expensiveHotels));
            await _dbContext.SaveChangesAsync();

            // Act
            var response = await _client.GetAsync("/api/v1/hotels?page=1&pageSize=10&minPrice=100&maxPrice=200");
            var result = response.Content.Headers.ContentType?.MediaType == "application/json" ?
                await response.Content.ReadFromJsonAsync<ResultWithPaginationResponse<IEnumerable<HotelDto>>>() : null;

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            result.Should().NotBeNull();
            result?.Results.Should().HaveCount(3);
            result?.TotalRecords.Should().Be(3);
            result?.Page.Should().Be(1);
            result?.PageSize.Should().Be(10);
            result?.Results.All(h => h.PricePerNight >= 100 && h.PricePerNight <= 200).Should().BeTrue();
        }

        [Fact]
        public async Task GetHotels_Should_ReturnHotelsFilteredByMultipleCriteria()
        {
            // Arrange
            var city = _fixture.Build<City>()
                .With(x => x.Name, "New York")
                .Without(x => x.Hotels)
                .Create();
            var amenity = _fixture.Build<Amenity>()
                .With(a => a.Id, 10)
                .With(a => a.Name, "Free WiFi")
                .Without(a => a.HotelAmenity)
                .Create();

            var otherHotels = _fixture.CreateMany<Hotel>(5);
            foreach (var item in otherHotels)
            {
                item.PricePerNight = 500;
            }
            var hotels = _fixture.CreateMany<Hotel>(2);
            foreach (var item in hotels)
            {
                item.City = city;
                item.HotelAmenity = [new HotelAmenity { AmenityId = 10, HotelId = item.Id }];
                item.PricePerNight = 150;
            }
            await _dbContext.Amenities.AddAsync(amenity);
            await _dbContext.Hotels.AddRangeAsync(hotels.Concat(otherHotels));
            await _dbContext.SaveChangesAsync();

            // Act
            var response = await _client.GetAsync("/api/v1/hotels?page=1&pageSize=10&city=New York&amenities=10&minPrice=100&maxPrice=200");
            var result = response.Content.Headers.ContentType?.MediaType == "application/json" ?
                await response.Content.ReadFromJsonAsync<ResultWithPaginationResponse<IEnumerable<HotelDto>>>() : null;

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            result.Should().NotBeNull();
            result?.Results.Should().HaveCount(2);
            result?.TotalRecords.Should().Be(2);
            result?.Page.Should().Be(1);
            result?.PageSize.Should().Be(10);
            result?.Results.All(h => h.City == "New York" && h.Amenities.Any(ha => ha.Name == "Free WiFi") && h.PricePerNight >= 100 && h.PricePerNight <= 200).Should().BeTrue();
        }
    }
}
