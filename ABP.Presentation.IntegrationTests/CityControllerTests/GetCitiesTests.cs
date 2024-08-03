using ABPIntegrationTests;
using Application.Dtos.CityDtos;
using AutoFixture;
using Domain.Entities;
using FluentAssertions;
using Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Presentation.Responses.Pagination;
using System.Net;
using System.Net.Http.Json;

namespace ABP.Presentation.IntegrationTests.CityControllerTests
{
    public class GetCitiesTests : IClassFixture<ABPWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly Fixture _fixture;
        private readonly ApplicationDbContext _dbContext;

        public GetCitiesTests(ABPWebApplicationFactory factory)
        {
            factory.DatabaseName = Guid.NewGuid().ToString();
            _client = factory.CreateClient();
            _fixture = new Fixture();
            var scope = factory.Services.CreateScope();
            _dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            factory.SetupDbContext(_dbContext).GetAwaiter().GetResult();
        }

        [Fact]
        public async Task GetCities_Should_ReturnPaginatedCities_WhenSuccess()
        {
            // Arrange
            var cities = _fixture.Build<City>()
                .Without(c => c.Hotels)
                .CreateMany(5);
            await _dbContext.Cities.AddRangeAsync(cities);
            await _dbContext.SaveChangesAsync();

            // Act
            var response = await _client.GetAsync("/api/cities?page=1&pageSize=10");
            var result = response.Content.Headers.ContentType?.MediaType == "application/json" ?
                await response.Content.ReadFromJsonAsync<ResultWithPaginationResponse<IEnumerable<CityDto>>>() : null;

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
        public async Task GetCities_Should_ReturnEmptyList_WhenNoCitiesExist()
        {
            // Act
            var response = await _client.GetAsync("/api/cities?page=1&pageSize=10");
            var result = response.Content.Headers.ContentType?.MediaType == "application/json" ?
                await response.Content.ReadFromJsonAsync<ResultWithPaginationResponse<IEnumerable<CityDto>>>() : null;

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
        public async Task GetCities_Should_ReturnEmptyList_WhenPageIsOutOfRange()
        {
            // Arrange
            var cities = _fixture.Build<City>()
                .Without(c => c.Hotels)
                .CreateMany(5);
            await _dbContext.Cities.AddRangeAsync(cities);
            await _dbContext.SaveChangesAsync();

            // Act
            var response = await _client.GetAsync("/api/cities?page=2&pageSize=10");
            var result = response.Content.Headers.ContentType?.MediaType == "application/json" ?
                await response.Content.ReadFromJsonAsync<ResultWithPaginationResponse<IEnumerable<CityDto>>>() : null;

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
        public async Task GetCities_Should_ReturnCities_WithDifferentPageSizes()
        {
            // Arrange
            var cities = _fixture.Build<City>()
                .Without(c => c.Hotels)
                .CreateMany(15);
            await _dbContext.Cities.AddRangeAsync(cities);
            await _dbContext.SaveChangesAsync();

            // Act & Assert for PageSize=5
            var responsePageSize5 = await _client.GetAsync("/api/cities?page=1&pageSize=5");
            var resultPageSize5 = responsePageSize5.Content.Headers.ContentType?.MediaType == "application/json" ?
                await responsePageSize5.Content.ReadFromJsonAsync<ResultWithPaginationResponse<IEnumerable<CityDto>>>() : null;
            responsePageSize5.StatusCode.Should().Be(HttpStatusCode.OK);
            resultPageSize5.Should().NotBeNull();
            resultPageSize5?.Results.Should().HaveCount(5);
            resultPageSize5?.TotalRecords.Should().Be(15);
            resultPageSize5?.Page.Should().Be(1);
            resultPageSize5?.PageSize.Should().Be(5);

            // Act & Assert for PageSize=10
            var responsePageSize10 = await _client.GetAsync("/api/cities?page=1&pageSize=10");
            var resultPageSize10 = responsePageSize10.Content.Headers.ContentType?.MediaType == "application/json" ?
                await responsePageSize10.Content.ReadFromJsonAsync<ResultWithPaginationResponse<IEnumerable<CityDto>>>() : null;
            responsePageSize10.StatusCode.Should().Be(HttpStatusCode.OK);
            resultPageSize10.Should().NotBeNull();
            resultPageSize10?.Results.Should().HaveCount(10);
            resultPageSize10?.TotalRecords.Should().Be(15);
            resultPageSize10?.Page.Should().Be(1);
            resultPageSize10?.PageSize.Should().Be(10);
        }

        [Fact]
        public async Task GetCities_Should_ReturnCities_WhenInvalidPageParameters()
        {
            // Arrange
            var cities = _fixture.Build<City>()
                .Without(c => c.Hotels)
                .CreateMany(15);
            await _dbContext.Cities.AddRangeAsync(cities);
            await _dbContext.SaveChangesAsync();

            // Act
            var response = await _client.GetAsync("/api/cities?page=-1&pageSize=0");
            var result = response.Content.Headers.ContentType?.MediaType == "application/json" ?
                await response.Content.ReadFromJsonAsync<ResultWithPaginationResponse<IEnumerable<CityDto>>>() : null;

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
        public async Task GetCities_Should_HandleLargeNumberOfCities()
        {
            // Arrange
            var cities = _fixture.Build<City>()
                .Without(c => c.Hotels)
                .CreateMany(1000);
            await _dbContext.Cities.AddRangeAsync(cities);
            await _dbContext.SaveChangesAsync();

            // Act
            var response = await _client.GetAsync("/api/cities?page=10&pageSize=100");
            var result = response.Content.Headers.ContentType?.MediaType == "application/json" ?
                await response.Content.ReadFromJsonAsync<ResultWithPaginationResponse<IEnumerable<CityDto>>>() : null;

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
        public async Task GetCities_Should_ReturnCitiesFilteredByCountry()
        {
            // Arrange
            var cities = _fixture.Build<City>()
                .With(c => c.Country, "USA")
                .Without(c => c.Hotels)
                .CreateMany(5);
            var otherCities = _fixture.Build<City>()
                .With(c => c.Country, "Canada")
                .Without(c => c.Hotels)
                .CreateMany(3);
            await _dbContext.Cities.AddRangeAsync(cities.Concat(otherCities));
            await _dbContext.SaveChangesAsync();

            // Act
            var response = await _client.GetAsync("/api/cities?page=1&pageSize=10&country=USA");
            var result = response.Content.Headers.ContentType?.MediaType == "application/json" ?
                await response.Content.ReadFromJsonAsync<ResultWithPaginationResponse<IEnumerable<CityDto>>>() : null;

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            result.Should().NotBeNull();
            result?.Results.Should().HaveCount(5);
            result?.TotalRecords.Should().Be(5);
            result?.Page.Should().Be(1);
            result?.PageSize.Should().Be(10);
            result?.Results.All(c => c.Country == "USA").Should().BeTrue();
        }

        [Fact]
        public async Task GetCities_Should_ReturnCitiesFilteredByCityName()
        {
            // Arrange
            var matchingCity = _fixture.Build<City>()
                .With(c => c.Name, "New York")
                .Without(c => c.Hotels)
                .Create();
            var otherCities = _fixture.Build<City>()
                .Without(c => c.Hotels)
                .CreateMany(4);
            await _dbContext.Cities.AddRangeAsync(new[] { matchingCity }.Concat(otherCities));
            await _dbContext.SaveChangesAsync();

            // Act
            var response = await _client.GetAsync("/api/cities?page=1&pageSize=10&city=New York");
            var result = response.Content.Headers.ContentType?.MediaType == "application/json" ?
                await response.Content.ReadFromJsonAsync<ResultWithPaginationResponse<IEnumerable<CityDto>>>() : null;
            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            result.Should().NotBeNull();
            result?.Results.Should().HaveCount(1);
            result?.TotalRecords.Should().Be(1);
            result?.Page.Should().Be(1);
            result?.PageSize.Should().Be(10);
            result?.Results.First().Name.Should().Be("New York");
        }
    }
}
