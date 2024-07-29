using Application.CommandsAndQueries.CityCQ.Query.GetCities;
using Application.Dtos.CityDtos;
using Application.Execptions;
using AutoFixture;
using Domain.Abstractions;
using FluentAssertions;
using Moq;

namespace ABP.Application.Tests.CityTests.Queries
{
    public class GetCitiesHandlerTests
    {
        private readonly Mock<ICityRepository> _cityRepositoryMock;
        private readonly IFixture _fixture;
        private readonly GetCitiesQuery _query;
        private readonly GetCitiesHandler _handler;
        public GetCitiesHandlerTests()
        {
            _cityRepositoryMock = new();
            _fixture = new Fixture();
            int page = _fixture.Create<int>();
            int pageSize = _fixture.Create<int>();
            _query = new(page, pageSize);
            _handler = new GetCitiesHandler(_cityRepositoryMock.Object);
        }
        [Fact]
        public async Task Handler_Should_ReturnCities_WhenSuccess()
        {
            //Arrange
            //Act
            (IEnumerable<CityDto> result, int totalRecords, int page, int pageSize) =
                await _handler.Handle(_query, default);
            //Assert
            _cityRepositoryMock.Verify(
                x =>
                    x.GetAsync(
                        It.IsAny<int>(), It.IsAny<int>(), default, default
                    ),
               Times.Once
            );
            result.Should().NotBeNull();
            totalRecords.Should().BeGreaterThanOrEqualTo(0);
            page.Should().BeGreaterThanOrEqualTo(1);
            pageSize.Should().BeInRange(1, 100);
        }
        [Fact]
        public async Task Handler_Should_ThrowsException_WhenFail()
        {
            //Arrange
            _cityRepositoryMock.Setup(x =>
                x.GetAsync(
                    It.IsAny<int>(), It.IsAny<int>(), default, default
                )
            ).Throws<Exception>();
            //Act
            Func<Task> result = async () => await _handler.Handle(_query, default);
            //Assert
            await result.Should().ThrowAsync<ErrorException>();
        }
    }
}