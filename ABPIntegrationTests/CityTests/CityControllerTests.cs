using Application.CommandsAndQueries.CityCQ.Commands.Create;
using Application.CommandsAndQueries.CityCQ.Commands.Update;
using Application.Dtos.CityDtos;
using FluentAssertions;
using Presentation.Responses.NotFound;
using Presentation.Responses.Pagination;
using Presentation.Responses.Validation;
using System.Net;
using System.Net.Http.Json;
namespace ABPIntegrationTests.CityTests
{
    [TestCaseOrderer(
    ordererTypeName: "ABPIntegrationTests.PriorityOrderer",
    ordererAssemblyName: "ABPIntegrationTests")]
    public class CityControllerTests : IClassFixture<ABPWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private static CityDto? _city;
        private readonly string skipMessage = "Skipping due to create city test failure";
        public CityControllerTests(ABPWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact, TestPriority(1)]
        public async Task CreateCity_ReturnsCreatedAtAction_WithNewCity()
        {
            //Arrange
            var newCity = new CreateCityCommand { Name = "Test City", Country = "Test Country", PostOffice = "100" };
            //Act
            var response = await _client.PostAsJsonAsync("/api/cities", newCity);
            var createdCity = await response.Content.ReadFromJsonAsync<CityDto>();
            _city = createdCity;
            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            createdCity.Should().NotBeNull();
            createdCity?.Id.Should().BeGreaterThanOrEqualTo(1);
            createdCity?.Name.Should().Be(newCity.Name);
            createdCity?.Country.Should().Be(newCity.Country);
        }

        [Theory, TestPriority(2)]
        [MemberData(nameof(CityTestData.CreateCityTestData), MemberType = typeof(CityTestData))]
        public async Task CreateCity_ReturnsBadRequest_WhenInvalidBody(CreateCityCommand? command)
        {
            Skip.If(_city is null, skipMessage);
            //Act
            var response = await _client.PostAsJsonAsync("/api/cities", command);
            var badRequest = await response.Content.ReadFromJsonAsync<ValidationFailureResponse>();

            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            badRequest.Should().NotBeNull();
        }

        [Fact, TestPriority(2)]
        public async Task GetCities_ReturnsOkResponse_WithListOfCities()
        {
            Skip.If(_city is null, skipMessage);
            //Act
            var response = await _client.GetAsync("/api/cities");

            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<ResultWithPaginationResponse<IEnumerable<CityDto>>>();
            result.Should().NotBeNull();
            result?.Results.Should().NotBeNull();
            result?.Results?.Should().Match((x) => x.Any(y => y.Id == _city.Id));
            result?.TotalRecords.Should().BeGreaterThanOrEqualTo(0);
            result?.PageSize.Should().BeGreaterThanOrEqualTo(1);
            result?.Page.Should().BeGreaterThanOrEqualTo(1);
        }

        [Fact, TestPriority(2)]
        public async Task GetCity_ReturnsOkResponse_WithCity()
        {
            Skip.If(_city is null, skipMessage);
            //Act
            var response = await _client.GetAsync($"/api/cities/{_city.Id}");

            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var city = await response.Content.ReadFromJsonAsync<CityDto>();
            city.Should().NotBeNull();
            city?.Id.Should().Be(_city.Id);
            city?.Name.Should().NotBeNull();
            city?.Country.Should().NotBeNull();
        }

        [Theory, TestPriority(2)]
        [MemberData(nameof(CityTestData.NotFoundTestData), MemberType = typeof(CityTestData))]
        public async Task GetCity_ReturnsNotFound_WhenCityNotExists(string id)
        {
            //Act
            var response = await _client.GetAsync($"/api/cities/{id}");
            var notFound = await response.Content.ReadFromJsonAsync<NotFoundResponse>();

            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            notFound.Should().NotBeNull();
        }

        [Fact, TestPriority(2)]
        public async Task UpdateCity_ReturnsOkResponse_WhenCityUpdated()
        {
            Skip.If(_city is null, skipMessage);
            //Arrange
            var updatedCity = new UpdateCityCommand
            {
                Name = "Updated City",
                Country = "Updated Country",
                PostOffice = "200"
            };
            //Act
            var response = await _client.PutAsJsonAsync($"/api/cities/{_city.Id}", updatedCity);
            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact, TestPriority(2)]
        public async Task UpdateCity_ReturnsOkResponse_WhenCityCommandIsNull()
        {
            Skip.If(_city is null, skipMessage);
            //Arrange
            UpdateCityCommand? updatedCity = null;
            //Act
            var response = await _client.PutAsJsonAsync($"/api/cities/{_city.Id}", updatedCity);
            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Theory, TestPriority(2)]
        [MemberData(nameof(CityTestData.UpdateCityTestData), MemberType = typeof(CityTestData))]
        public async Task UpdateCity_ReturnsBadRequest_WhenInvalidBody(UpdateCityCommand? command)
        {
            Skip.If(_city is null, skipMessage);
            //Act
            var response = await _client.PutAsJsonAsync($"/api/cities/{_city.Id}", command);
            var badRequest = await response.Content.ReadFromJsonAsync<ValidationFailureResponse>();

            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            badRequest.Should().NotBeNull();
        }

        [Theory, TestPriority(2)]
        [MemberData(nameof(CityTestData.NotFoundTestData), MemberType = typeof(CityTestData))]
        public async Task UpdateCity_ReturnsNotFound_WhenCityNotExists(string id)
        {
            Skip.If(_city is null, skipMessage);
            //Arrange
            var updatedCity = new UpdateCityCommand { Name = "Updated", Country = "Updated", PostOffice = "900" };
            //Act
            var response = await _client.PutAsJsonAsync($"/api/cities/{id}", updatedCity);
            var notFound = await response.Content.ReadFromJsonAsync<NotFoundResponse>();
            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            notFound.Should().NotBeNull();
        }

        [Fact, TestPriority(3)]
        public async Task DeleteCity_ReturnsOkResponse_WhenCityDeleted()
        {
            Skip.If(_city is null, skipMessage);
            //Act
            var response = await _client.DeleteAsync($"/api/cities/{_city.Id}");
            var deletedCity = await response.Content.ReadFromJsonAsync<CityDto>();

            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            deletedCity.Should().NotBeNull();
            deletedCity?.Id.Should().Be(_city.Id);
        }

        [Theory, TestPriority(3)]
        [MemberData(nameof(CityTestData.NotFoundTestData), MemberType = typeof(CityTestData))]
        public async Task DeleteCity_ReturnsNotFound_WhenCityNotExists(string id)
        {
            //Act
            var response = await _client.DeleteAsync($"/api/cities/{id}");
            var notFound = await response.Content.ReadFromJsonAsync<NotFoundResponse>();
            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            notFound.Should().NotBeNull();
        }
    }
}
