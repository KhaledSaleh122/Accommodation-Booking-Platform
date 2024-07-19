using Application.CommandsAndQueries.AmenityCQ.Commands.Create;
using Application.CommandsAndQueries.AmenityCQ.Commands.Update;
using Application.Dtos.AmenityDtos;
using FluentAssertions;
using Presentation.Responses.NotFound;
using Presentation.Responses.Pagination;
using Presentation.Responses.Validation;
using System.Net;
using System.Net.Http.Json;
namespace ABPIntegrationTests.AmenityTests
{
    [TestCaseOrderer(
    ordererTypeName: "ABPIntegrationTests.PriorityOrderer",
    ordererAssemblyName: "ABPIntegrationTests")]
    public class AmenityControllerTests : IClassFixture<ABPWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly static AmenityDto _amenity = new();
        private readonly string skipMessage = "Skipping due to create amenity test failure";
        public AmenityControllerTests(ABPWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact, TestPriority(1)]
        public async Task CreateAmenity_ReturnsCreatedAtAction_WithNewAmenity()
        {
            //Arrange
            var newAmenity = new CreateAmenityCommand { Name = "Test", Description = "Test2" };
            //Act
            var response = await _client.PostAsJsonAsync("/api/amenities", newAmenity);
            var createdAmenity = await response.Content.ReadFromJsonAsync<AmenityDto>();
            _amenity.Id = createdAmenity?.Id ?? 0;
            _amenity.Name = createdAmenity?.Name ?? string.Empty;
            _amenity.Description = createdAmenity?.Description ?? string.Empty;

            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            createdAmenity.Should().NotBeNull();
            createdAmenity?.Id.Should().BeGreaterThanOrEqualTo(1);
            createdAmenity?.Name.Should().Be(newAmenity.Name);
            createdAmenity?.Description.Should().Be(newAmenity.Description);
        }

        [Theory, TestPriority(1)]
        [MemberData(nameof(AmenityTestData.CreateAmenityTestData), MemberType = typeof(AmenityTestData))]

        public async Task CreateAmenity_ReturnsBadRequest_WhenInvalidBody(CreateAmenityCommand? command)
        {
            //Act
            var response = await _client.PostAsJsonAsync("/api/amenities", command);
            var badRequest = await response.Content.ReadFromJsonAsync<ValidationFailureResponse>();

            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            badRequest.Should().NotBeNull();
        }


        [Fact, TestPriority(2)]
        public async Task GetAmenities_ReturnsOkResponse_WithListOfAmenities()
        {
            Skip.If(_amenity.Id == 0, skipMessage);
            //Act
            var response = await _client.GetAsync("/api/amenities");

            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<ResultWithPaginationResponse<IEnumerable<AmenityDto>>>();
            result.Should().NotBeNull();
            result?.Results.Should().NotBeNull();
            result?.Results?.Should().Match((x) => x.Any(y => y.Id == _amenity.Id));
            result?.TotalRecords.Should().BeGreaterThanOrEqualTo(0);
            result?.PageSize.Should().BeGreaterThanOrEqualTo(1);
            result?.Page.Should().BeGreaterThanOrEqualTo(1);
        }

        [Fact, TestPriority(2)]
        public async Task GetAmenity_ReturnsOkResponse_WithAmenity()
        {
            Skip.If(_amenity.Id == 0, skipMessage);
            //Act
            var response = await _client.GetAsync($"/api/amenities/{_amenity.Id}");

            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var amenity = await response.Content.ReadFromJsonAsync<AmenityDto>();
            amenity.Should().NotBeNull();
            amenity?.Id.Should().Be(1);
            amenity?.Name.Should().NotBeNull();
            amenity?.Description.Should().NotBeNull();
        }

        [Theory, TestPriority(2)]
        [MemberData(nameof(AmenityTestData.NotFoundTestData), MemberType = typeof(AmenityTestData))]
        public async Task GetAmenity_ReturnsNotFound_WhenAmenityNotExists(string x)
        {
            //Act
            var response = await _client.GetAsync($"/api/amenities/{x}");
            var notFound = await response.Content.ReadFromJsonAsync<NotFoundResponse>();

            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            notFound.Should().NotBeNull();
        }


        [Fact, TestPriority(2)]
        public async Task UpdateAmenity_ReturnsOkResponse_WhenAmenityUpdated()
        {
            Skip.If(_amenity.Id == 0, skipMessage);
            //Arrange
            var updatedAmenity = new UpdateAmenityCommand
            {
                Name = "Updated Amenity",
                Description = "Updated Description"
            };
            //Act
            var response = await _client.PutAsJsonAsync($"/api/amenities/{_amenity.Id}", updatedAmenity);
            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact, TestPriority(2)]
        public async Task UpdateAmenity_ReturnsOkResponse_WhenAmenityCommandIsNull()
        {
            Skip.If(_amenity.Id == 0, skipMessage);
            //Arrange
            UpdateAmenityCommand? updatedAmenity = null;
            //Act
            var response = await _client.PutAsJsonAsync($"/api/amenities/{_amenity.Id}", updatedAmenity);
            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Theory, TestPriority(2)]
        [MemberData(nameof(AmenityTestData.UpdateAmenityTestData), MemberType = typeof(AmenityTestData))]

        public async Task UpdateAmenity_ReturnsBadRequest_WhenInvalidBody(UpdateAmenityCommand? command)
        {
            Skip.If(_amenity.Id == 0, skipMessage);
            //Act
            var response = await _client.PutAsJsonAsync($"/api/amenities/{_amenity.Id}", command);
            var badRequest = await response.Content.ReadFromJsonAsync<ValidationFailureResponse>();

            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            badRequest.Should().NotBeNull();
        }

        [Theory, TestPriority(2)]
        [MemberData(nameof(AmenityTestData.NotFoundTestData), MemberType = typeof(AmenityTestData))]
        public async Task UpdateAmenity_ReturnsNotFound_WhenAmenityNotExists(string id)
        {
            Skip.If(_amenity.Id == 0, skipMessage);
            //Arrange
            var updatedAmenity = new UpdateAmenityCommand { Name = "Updated", Description = "Updated" };
            //Act
            var response = await _client.PutAsJsonAsync($"/api/amenities/{id}", updatedAmenity);
            var notFound = await response.Content.ReadFromJsonAsync<NotFoundResponse>();
            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            notFound.Should().NotBeNull();
        }



        [Fact, TestPriority(3)]
        public async Task DeleteAmenity_ReturnsOkResponse_WhenAmenityDeleted()
        {
            Skip.If(_amenity.Id == 0, skipMessage);
            //Act
            var response = await _client.DeleteAsync($"/api/amenities/{_amenity.Id}");
            var deletedAmenity = await response.Content.ReadFromJsonAsync<AmenityDto>();

            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            deletedAmenity.Should().NotBeNull();
            deletedAmenity?.Id.Should().Be(1);
        }

        [Theory, TestPriority(3)]
        [MemberData(nameof(AmenityTestData.NotFoundTestData), MemberType = typeof(AmenityTestData))]
        public async Task DeleteAmenity_ReturnsNotFound_WhenAmenityNotExists(string id)
        {
            //Act
            var response = await _client.DeleteAsync($"/api/amenities/{id}");
            var notFound = await response.Content.ReadFromJsonAsync<NotFoundResponse>();
            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            notFound.Should().NotBeNull();
        }
    }
}
