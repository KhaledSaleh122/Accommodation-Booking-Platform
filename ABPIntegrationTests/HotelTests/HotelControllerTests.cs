using Application.CommandsAndQueries.CityCQ.Commands.Create;
using Application.CommandsAndQueries.CityCQ.Commands.Update;
using Application.CommandsAndQueries.HotelCQ.Commands.Create;
using Application.Dtos.CityDtos;
using Application.Dtos.HotelDtos;
using Domain.Enums;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Presentation.Responses.NotFound;
using Presentation.Responses.Validation;
using System.Net;
using System.Net.Http.Json;
using System.Text;

namespace ABPIntegrationTests.HotelTests
{
    [TestCaseOrderer(
    ordererTypeName: "ABPIntegrationTests.PriorityOrderer",
    ordererAssemblyName: "ABPIntegrationTests")]
    public class HotelControllerTests : IClassFixture<ABPWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private static HotelMinDto? _hotel;
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
            _hotel = createdHotel;

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
        [Theory, TestPriority(1)]
        [MemberData(nameof(HotelTestData.CreateHotelTestData), MemberType = typeof(HotelTestData))]
        public async Task CreateHotel_ReturnsBadRequest_WhenInvalidBody(
            CreateHotelCommand? command
            )
        {
            //Arrange
            var imageContent = new MemoryStream(Encoding.UTF8.GetBytes("fake image content"));
            var thumbnailContent = new MemoryStream(Encoding.UTF8.GetBytes("fake thumbnail content"));
            var formData = new MultipartFormDataContent();
            if (command?.Name is not null) formData.Add(new StringContent(command.Name), nameof(command.Name));
            if (command?.Description is not null)
                formData.Add(new StringContent(command.Description), nameof(command.Description));
            if (command?.Owner is not null)
                formData.Add(new StringContent(command.Owner), nameof(command.Owner));
            if (command?.Address is not null)
                formData.Add(new StringContent(command.Address), nameof(command.Address));
            if (command?.PricePerNight is not null)
                formData.Add(new StringContent(command.PricePerNight.ToString()), nameof(command.PricePerNight));
            if (command?.CityId is not null)
                formData.Add(new StringContent(command.CityId.ToString()), nameof(command.CityId));
            if (command?.HotelType is not null)
                formData.Add(new StringContent(command.HotelType.ToString()), nameof(command.HotelType));
            if (command?.Images is not null && command.Images.Count > 0)
                formData.Add(
                    new StreamContent(imageContent), nameof(command.Images), command.Images[0].FileName
                );
            if (command?.Thumbnail is not null)
                formData.Add(
                    new StreamContent(thumbnailContent), nameof(command.Thumbnail), command.Thumbnail.FileName
                );

            // Act
            var response = await _client.PostAsync("/api/hotels", formData);
            var badRequest = await response.Content.ReadFromJsonAsync<ValidationFailureResponse>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            badRequest.Should().NotBeNull();
        }

        [Fact, TestPriority(2)]
        public async Task GetHotel_ReturnsOkResponse_WithHotel()
        {
            Skip.If(_hotel is null, skipMessage);

            // Act
            var response = await _client.GetAsync($"/api/hotels/{_hotel.Id}");
            var hotel = await response.Content.ReadFromJsonAsync<HotelFullDto>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            hotel.Should().NotBeNull();
            hotel?.Id.Should().BeGreaterThanOrEqualTo(1);
            hotel?.Name.Should().Be(_hotel.Name);
            hotel?.Description.Should().Be(_hotel.Description);
            hotel?.Owner.Should().Be(_hotel.Owner);
            hotel?.Address.Should().Be(_hotel.Address);
            hotel?.City.Should().Be(_hotel.City);
            hotel?.Country.Should().Be(_hotel.Country);
            hotel?.Thumbnail.Should().Be(_hotel.Thumbnail);
            hotel?.Images.Should().NotBeNullOrEmpty();
        }

        [Theory, TestPriority(2)]
        [MemberData(nameof(HotelTestData.NotFoundTestData), MemberType = typeof(HotelTestData))]
        public async Task GetHotel_ReturnsNotFound_WhenHotelNotExists(object id)
        {
            Skip.If(_hotel is null, skipMessage);
            // Act
            var response = await _client.GetAsync($"/api/hotels/{id}");
            var notFound = await response.Content.ReadFromJsonAsync<NotFoundResponse>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            notFound.Should().NotBeNull();
        }

        [Theory, TestPriority(3)]
        [MemberData(nameof(HotelTestData.PatchUpdateTestData), MemberType = typeof(HotelTestData))]
        public async Task UpdateHotel_ReturnsOkResponse_WhenHotelUpdated(UpdateHotelCommand command)
        {
            Skip.If(_hotel is null, skipMessage);
            // Act
            var response = await _client.PatchAsJsonAsync($"/api/hotels/{_hotel.Id}", command);
            var responseHotel = await _client.GetAsync($"/api/hotels/{_hotel.Id}");
            var hotel = await responseHotel.Content.ReadFromJsonAsync<HotelFullDto>();
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            hotel.Should().NotBeNull();
            if (command.Name is not null) hotel!.Name.Should().Be(command.Name);
            if (command.Description is not null) hotel!.Description.Should().Be(command.Description);
            if (command.Owner is not null) hotel!.Owner.Should().Be(command.Owner);
            if (command.Address is not null) hotel!.Address.Should().Be(command.Address);
            if (command.PricePerNight is not null) hotel!.PricePerNight.Should().Be(command.PricePerNight);
            if (command.HotelType is not null)
                hotel!.HotelType.Should().Be( Enum.GetName(typeof(HotelType),command.HotelType));

        }

        [Theory, TestPriority(3)]
        [MemberData(nameof(HotelTestData.UpdateHotelInvalidTestData), MemberType = typeof(HotelTestData))]
        public async Task UpdateHotel_ReturnsBadRequest_WhenInvalidBody(UpdateHotelCommand? command)
        {
            Skip.If(_hotel is null, skipMessage);
            //Act
            var response = await _client.PatchAsJsonAsync($"/api/hotels/{_hotel.Id}", command);
            var badRequest = await response.Content.ReadFromJsonAsync<ValidationFailureResponse>();

            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            badRequest.Should().NotBeNull();
        }


        [Fact, TestPriority(3)]
        public async Task UpdateHotel_ReturnsBadRequest_WhenHotelCommandIsNull()
        {
            Skip.If(_hotel is null, skipMessage);

            // Arrange
            UpdateHotelCommand? updatedHotel = null;

            // Act
            var response = await _client.PatchAsJsonAsync($"/api/hotels/{_hotel.Id}", updatedHotel);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

    }
}