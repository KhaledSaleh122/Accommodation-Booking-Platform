using ABPIntegrationTests;
using Application.Dtos.AmenityDtos;
using AutoFixture;
using Domain.Entities;
using FluentAssertions;
using Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Presentation.Responses.Pagination;
using System.Net;
using System.Net.Http.Json;

namespace ABP.Presentation.IntegrationTests.AmenityControllerTests
{
    public class GetAmenitiesTests : IClassFixture<ABPWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly Fixture _fixture;
        private readonly ApplicationDbContext _dbContext;

        public GetAmenitiesTests(ABPWebApplicationFactory factory)
        {
            factory.DatabaseName = $"InMemoryDb_{Guid.NewGuid()}";
            _client = factory.CreateClient();
            _fixture = new Fixture();
            var scope = factory.Services.CreateScope();
            factory.SetupDbContext(scope).GetAwaiter().GetResult();
            _dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        }

        [Fact]
        public async Task GetAmenities_Should_ReturnPaginatedAmenities_WhenSuccess()
        {
            // Arrange
            var amenities = _fixture.Build<Amenity>()
                .Without(c => c.HotelAmenity)
                .CreateMany(5);
            await _dbContext.Amenities.AddRangeAsync(amenities);
            await _dbContext.SaveChangesAsync();

            // Act
            var response = await _client.GetAsync("/api/amenities?page=1&pageSize=10");
            var result = response.Content.Headers.ContentType?.MediaType == "application/json" ?
                await response.Content.ReadFromJsonAsync<ResultWithPaginationResponse<IEnumerable<AmenityDto>>>() : null;

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
        public async Task GetAmenities_Should_ReturnEmptyList_WhenNoAmenitiesExist()
        {
            // Act
            var response = await _client.GetAsync("/api/amenities?page=1&pageSize=10");
            var result = response.Content.Headers.ContentType?.MediaType == "application/json" ?
                await response.Content.ReadFromJsonAsync<ResultWithPaginationResponse<IEnumerable<AmenityDto>>>() : null;

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
        public async Task GetAmenities_Should_ReturnEmptyList_WhenPageIsOutOfRange()
        {
            // Arrange
            var amenities = _fixture.Build<Amenity>()
                .Without(c => c.HotelAmenity)
                .CreateMany(5);
            await _dbContext.Amenities.AddRangeAsync(amenities);
            await _dbContext.SaveChangesAsync();

            // Act
            var response = await _client.GetAsync("/api/amenities?page=2&pageSize=10");
            var result = response.Content.Headers.ContentType?.MediaType == "application/json" ?
                await response.Content.ReadFromJsonAsync<ResultWithPaginationResponse<IEnumerable<AmenityDto>>>() : null;

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
        public async Task GetAmenities_Should_ReturnAmenities_WithDifferentPageSizes()
        {
            // Arrange
            var amenities = _fixture.Build<Amenity>()
                .Without(c => c.HotelAmenity)
                .CreateMany(15);
            await _dbContext.Amenities.AddRangeAsync(amenities);
            await _dbContext.SaveChangesAsync();

            // Act & Assert for PageSize=5
            var responsePageSize5 = await _client.GetAsync("/api/amenities?page=1&pageSize=5");
            var resultPageSize5 = responsePageSize5.Content.Headers.ContentType?.MediaType == "application/json" ?
                await responsePageSize5.Content.ReadFromJsonAsync<ResultWithPaginationResponse<IEnumerable<AmenityDto>>>() : null;
            responsePageSize5.StatusCode.Should().Be(HttpStatusCode.OK);
            resultPageSize5.Should().NotBeNull();
            resultPageSize5?.Results.Should().HaveCount(5);
            resultPageSize5?.TotalRecords.Should().Be(15);
            resultPageSize5?.Page.Should().Be(1);
            resultPageSize5?.PageSize.Should().Be(5);

            // Act & Assert for PageSize=10
            var responsePageSize10 = await _client.GetAsync("/api/amenities?page=1&pageSize=10");
            var resultPageSize10 = responsePageSize10.Content.Headers.ContentType?.MediaType == "application/json" ?
                await responsePageSize10.Content.ReadFromJsonAsync<ResultWithPaginationResponse<IEnumerable<AmenityDto>>>() : null;
            responsePageSize10.StatusCode.Should().Be(HttpStatusCode.OK);
            resultPageSize10.Should().NotBeNull();
            resultPageSize10?.Results.Should().HaveCount(10);
            resultPageSize10?.TotalRecords.Should().Be(15);
            resultPageSize10?.Page.Should().Be(1);
            resultPageSize10?.PageSize.Should().Be(10);
        }

        [Fact]
        public async Task GetAmenities_Should_ReturnAmenities_WhenInvalidPageParameters()
        {
            // Arrange
            var amenities = _fixture.Build<Amenity>()
                .Without(c => c.HotelAmenity)
                .CreateMany(15); 
            await _dbContext.Amenities.AddRangeAsync(amenities);
            await _dbContext.SaveChangesAsync();
            // Act
            var response = await _client.GetAsync("/api/amenities?page=-1&pageSize=0");
            var result = response.Content.Headers.ContentType?.MediaType == "application/json" ?
                await response.Content.ReadFromJsonAsync<ResultWithPaginationResponse<IEnumerable<AmenityDto>>>() : null;

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
        public async Task GetAmenities_Should_HandleLargeNumberOfAmenities()
        {
            // Arrange
            var amenities = _fixture.Build<Amenity>()
                .Without(c => c.HotelAmenity)
                .CreateMany(1000);
            await _dbContext.Amenities.AddRangeAsync(amenities);
            await _dbContext.SaveChangesAsync();

            // Act
            var response = await _client.GetAsync("/api/amenities?page=10&pageSize=100");
            var result = response.Content.Headers.ContentType?.MediaType == "application/json" ?
                await response.Content.ReadFromJsonAsync<ResultWithPaginationResponse<IEnumerable<AmenityDto>>>() : null;

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            result.Should().NotBeNull();
            result?.Results.Should().HaveCount(100);
            result?.TotalRecords.Should().Be(1000);
            result?.Page.Should().Be(10);
            result?.PageSize.Should().Be(100);
        }
    }
}
