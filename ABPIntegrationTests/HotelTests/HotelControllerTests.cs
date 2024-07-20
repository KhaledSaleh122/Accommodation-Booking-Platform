using Application.Dtos.HotelDtos;
using FluentAssertions;
using Presentation.Responses.NotFound;
using System.Net.Http.Json;
using System.Net;
using Application.CommandsAndQueries.HotelCQ.Commands.Create;
using Microsoft.AspNetCore.Http;
using System.Text;
using Application.CommandsAndQueries.CityCQ.Commands.Create;
using Application.Dtos.CityDtos;

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

        [Fact, TestPriority(1)]
        public async Task CreateHotel_ReturnsCreatedAtAction_WithNewHotel()
        {
            // Arrange
            var imageContent = Encoding.UTF8.GetBytes("fake image content");
            var thumbnailContent = Encoding.UTF8.GetBytes("fake thumbnail content");
            var newCity = new CreateCityCommand { Name = "Test City", Country = "Test Country", PostOffice = "100" };
            var responseCity = await _client.PostAsJsonAsync("/api/cities", newCity);
            var createdCity = await responseCity.Content.ReadFromJsonAsync<CityDto>();
            Skip.If(createdCity is null);
            var newHotel = new CreateHotelCommand
            {
                Name = "Test Hotel",
                Description = "Test Description",
                Owner = "Owner",
                Address = "Address",
                CityId = createdCity.Id,
                PricePerNight = 100,
                HotelType = 1,
            };
            var formData = new MultipartFormDataContent
            {
                { new StringContent(newHotel.Name), nameof(newHotel.Name) },
                { new StringContent(newHotel.Description), nameof(newHotel.Description) },
                { new StringContent(newHotel.Owner), nameof(newHotel.Owner) },
                { new StringContent(newHotel.Address), nameof(newHotel.Address) },
                { new StringContent(newHotel.CityId.ToString()), nameof(newHotel.CityId) },
                { new StringContent(newHotel.PricePerNight.ToString()), nameof(newHotel.PricePerNight) },
                { new StringContent(newHotel.HotelType.ToString()), nameof(newHotel.HotelType) },
                { new StreamContent(new MemoryStream(imageContent)), "Images", "test.jpg" },
                { new StreamContent(new MemoryStream(thumbnailContent)), "Thumbnail", "thumb.jpg" }
            };

            // Act
            var response = await _client.PostAsync("/api/hotels", formData);
            var createdHotel = await response.Content.ReadFromJsonAsync<HotelMinDto>();
            _hotel.Id = createdHotel?.Id ?? 0;
            _hotel.Name = createdHotel?.Name ?? string.Empty;
            _hotel.Description = createdHotel?.Description ?? string.Empty;

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            createdHotel.Should().NotBeNull();
            createdHotel?.Id.Should().BeGreaterThanOrEqualTo(1);
            createdHotel?.Name.Should().Be(newHotel.Name);
            createdHotel?.Description.Should().Be(newHotel.Description);
            createdHotel?.Owner.Should().Be(newHotel.Owner);
            createdHotel?.Address.Should().Be(newHotel.Address);
            createdHotel?.City.Should().Be(createdCity.Name);
            createdHotel?.Country.Should().Be(createdCity.Country);
            createdHotel?.Thumbnail.Should().NotBeNullOrEmpty();
            createdHotel?.Images.Should().NotBeNullOrEmpty();
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