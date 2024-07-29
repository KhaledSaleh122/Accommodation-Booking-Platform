using Application.CommandsAndQueries.AmenityCQ.Query.GetAmenities;
using Application.Dtos.AmenityDtos;
using Application.Execptions;
using AutoFixture;
using Domain.Abstractions;
using FluentAssertions;
using Moq;

namespace ABP.Application.Tests.AmenityTests.Queries
{
    public class GetAmenitiesHandlerTests
    {
        private readonly Mock<IAmenityRepository> _amenityRepositoryMock;
        private readonly IFixture _fixture;
        private readonly GetAmenitiesQuery _query;
        private readonly GetAmenitiesHandler _handler;

        public GetAmenitiesHandlerTests()
        {
            _amenityRepositoryMock = new();
            _fixture = new Fixture();
            int page = _fixture.Create<int>();
            int pageSize = _fixture.Create<int>();
            _query = new(page, pageSize);
            _handler = new GetAmenitiesHandler(_amenityRepositoryMock.Object);
        }
        [Fact]
        public async Task Handler_Should_ReturnAmenities_WhenSuccess()
        {
            //Arrange
            //Act
            (IEnumerable<AmenityDto> result, int totalRecords, int page, int pageSize) =
                await _handler.Handle(_query, default);
            //Assert
            _amenityRepositoryMock.Verify(x => x.GetAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Once);
            result.Should().NotBeNull();
            totalRecords.Should().BeGreaterThanOrEqualTo(0);
            page.Should().BeGreaterThanOrEqualTo(1);
            pageSize.Should().BeInRange(1, 100);
        }
        [Fact]
        public async Task Handler_Should_ThrowsException_WhenFail()
        {
            //Arrange
            _amenityRepositoryMock.Setup(x =>
                x.GetAsync(
                    It.IsAny<int>(), It.IsAny<int>()
                )
            ).Throws<Exception>();
            //Act
            Func<Task> result = async () => await _handler.Handle(_query, default);
            //Assert
            await result.Should().ThrowAsync<ErrorException>();
        }
    }
}