using Application.Dtos.HotelDtos;
using FluentAssertions;
using Presentation.Responses.NotFound;
using System.Net.Http.Json;
using System.Net;

namespace ABPIntegrationTests.HotelTests
{
    [TestCaseOrderer(
    ordererTypeName: "ABPIntegrationTests.PriorityOrderer",
    ordererAssemblyName: "ABPIntegrationTests")]
    public class HotelControllerTests : IClassFixture<ABPWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly static HotelDto _hotel = new();
        private readonly string skipMessage = "Skipping due to create hotel test failure";
        public HotelControllerTests(ABPWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Theory, TestPriority(2)]
        [MemberData(nameof(HotelTestData.NotFoundTestData), MemberType = typeof(HotelTestData))]
        public async Task GetHotel_ReturnsNotFound_WhenHotelNotExists(object id)
        {
            // Act
            var response = await _client.GetAsync($"/api/hotels/{id}");
            var notFound = await response.Content.ReadFromJsonAsync<NotFoundResponse>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            notFound.Should().NotBeNull();
        }
    }
}