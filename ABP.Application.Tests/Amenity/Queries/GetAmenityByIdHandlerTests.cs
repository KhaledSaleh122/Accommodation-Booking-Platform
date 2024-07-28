using Application.CommandsAndQueries.AmenityCQ.Query.GetAmenityById;
using Application.Dtos.AmenityDtos;
using Application.Exceptions;
using Application.Execptions;
using AutoFixture;
using Domain.Abstractions;
using Domain.Entities;
using FluentAssertions;
using Moq;

namespace ABP.Application.Tests
{
    public class GetAmenityByIdHandlerTests
    {
        private readonly Mock<IAmenityRepository> _amenityRepositoryMock;
        private readonly IFixture _fixture;
        private readonly GetAmenityByIdQuery _query;
        private readonly GetAmenityByIdHandler _handler;

        public GetAmenityByIdHandlerTests()
        {
            _amenityRepositoryMock = new();
            _fixture = new Fixture();
            _query = new GetAmenityByIdQuery()
            {
                AmenityId = _fixture.Create<int>()
            };
            _handler = new GetAmenityByIdHandler(_amenityRepositoryMock.Object);
        }
        [Fact]
        public async Task Handler_Should_ReturnRequestedAmenity_WhenSuccess()
        {
            //Arrange
            _amenityRepositoryMock.Setup(
                x => x.GetByIdAsync(It.IsAny<int>())
            ).ReturnsAsync(new Amenity());

            //Act
            AmenityDto result = (await _handler.Handle(_query, default))!;
            //Assert
            _amenityRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
            result.Should().NotBeNull();
        }
        [Fact]
        public async Task Handler_Should_ThrowsNotFoundException_WhenAmenityIsNull()
        {
            //Arrange
            _amenityRepositoryMock.Setup(
                x => x.GetByIdAsync(It.IsAny<int>())
            ).ReturnsAsync((Amenity?)null);

            //Act
            Func<Task> result = async () => await _handler.Handle(_query, default);
            //Assert
            _amenityRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Never);
            await result.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task Handler_Should_ThrowsException_WhenFail()
        {
            //Arrange

            _amenityRepositoryMock.Setup(
                x => x.GetByIdAsync(It.IsAny<int>())
            ).ReturnsAsync(new Amenity());
            _amenityRepositoryMock.Setup(x =>
                x.GetByIdAsync(
                    It.IsAny<int>()
                )
            ).Throws<Exception>();
            //Act
            Func<Task> result = async () => await _handler.Handle(_query, default);
            //Assert
            await result.Should().ThrowAsync<ErrorException>();
        }
    }
}