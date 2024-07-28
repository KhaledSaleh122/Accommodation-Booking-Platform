using Application.CommandsAndQueries.AmenityCQ.Commands.Update;
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
        public GetAmenityByIdHandlerTests()
        {
            _amenityRepositoryMock = new();
            _fixture = new Fixture();
            _query = new GetAmenityByIdQuery()
            {
                AmenityId = _fixture.Create<int>()
            };
        }
        [Fact]
        public async Task Handler_Should_ReturnDeletedAmenity_WhenSuccess()
        {
            //Arrange
            _amenityRepositoryMock.Setup(
                x => x.GetByIdAsync(It.IsAny<int>())
            ).ReturnsAsync(new Amenity());
            var handler = new GetAmenityByIdHandler(_amenityRepositoryMock.Object);
            //Act
            AmenityDto result = (await handler.Handle(_query, default))!;
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
            var handler = new GetAmenityByIdHandler(_amenityRepositoryMock.Object);
            //Act
            Func<Task> result = async () => await handler.Handle(_query, default);
            //Assert
            _amenityRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Never);
            await result.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task Handler_Should_ThrowsException_WhenFail()
        {
            //Arrange
            var handler = new GetAmenityByIdHandler(_amenityRepositoryMock.Object);
            _amenityRepositoryMock.Setup(
                x => x.GetByIdAsync(It.IsAny<int>())
            ).ReturnsAsync(new Amenity());
            _amenityRepositoryMock.Setup(x =>
                x.GetByIdAsync(
                    It.IsAny<int>()
                )
            ).Throws<Exception>();
            //Act
            Func<Task> result = async () => await handler.Handle(_query, default);
            //Assert
            await result.Should().ThrowAsync<ErrorException>();
        }
    }
}