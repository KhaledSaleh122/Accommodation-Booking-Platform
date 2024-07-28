using Application.CommandsAndQueries.AmenityCQ.Commands.Create;
using Application.CommandsAndQueries.AmenityCQ.Query.GetAmenities;
using Application.Dtos.AmenityDtos;
using Application.Execptions;
using AutoFixture;
using Domain.Abstractions;
using Domain.Entities;
using FluentAssertions;
using Moq;

namespace ABP.Application.Tests
{
    public class GetAmenitiesHandlerTests
    {
        private readonly Mock<IAmenityRepository> _amenityRepositoryMock;
        private readonly IFixture _fixture;
        private readonly GetAmenitiesQuery _query;
        public GetAmenitiesHandlerTests() {
            _amenityRepositoryMock = new();
            _fixture = new Fixture();
            int page = _fixture.Create<int>();
            int pageSize = _fixture.Create<int>();
            _query = new(page, pageSize);
        }
        [Fact]
        public async Task Handler_Should_ReturnAmenities_WhenSuccess()
        {
            //Arrange
            var handler = new GetAmenitiesHandler(_amenityRepositoryMock.Object);
            //Act
            (IEnumerable<AmenityDto> result, int totalRecords, int page, int pageSize) = 
                await handler.Handle(_query, default);
            //Assert
            _amenityRepositoryMock.Verify(x => x.GetAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Once);
            result.Should().NotBeNull();
            totalRecords.Should().BeGreaterThanOrEqualTo(0);
            page.Should().BeGreaterThanOrEqualTo(1);
            pageSize.Should().BeInRange(1,100);
        }
        [Fact]
        public async Task Handler_Should_ThrowsException_WhenFail() {
            //Arrange
            var handler = new GetAmenitiesHandler(_amenityRepositoryMock.Object);
            _amenityRepositoryMock.Setup(x =>
                x.GetAsync(
                    It.IsAny<int>(), It.IsAny<int>()
                )
            ).Throws<Exception>();
            //Act
            Func<Task> result = async () => await handler.Handle(_query, default);
            //Assert
            await result.Should().ThrowAsync<ErrorException>();
        }
    }
}