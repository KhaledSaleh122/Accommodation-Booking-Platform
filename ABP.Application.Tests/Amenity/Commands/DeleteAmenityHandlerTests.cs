using Application.CommandsAndQueries.AmenityCQ.Commands.Delete;
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
    public class DeleteAmenityHandlerTests
    {
        private readonly Mock<IAmenityRepository> _amenityRepositoryMock;
        private readonly IFixture _fixture;
        private readonly DeleteAmenityCommand _command;
        public DeleteAmenityHandlerTests()
        {
            _amenityRepositoryMock = new();
            _fixture = new Fixture();
            _command = new DeleteAmenityCommand()
            {
                Id = _fixture.Create<int>()
            };
        }
        [Fact]
        public async Task Handler_Should_ReturnDeletedAmenity_WhenSuccess()
        {
            //Arrange
            _amenityRepositoryMock.Setup(
                x => x.GetByIdAsync(It.IsAny<int>())
            ).ReturnsAsync(new Amenity());
            _amenityRepositoryMock.Setup(
                x => x.DeleteAsync(It.IsAny<Amenity>())
            ).ReturnsAsync(new Amenity());
            var handler = new DeleteAmenityHandler(_amenityRepositoryMock.Object);
            //Act
            AmenityDto result = await handler.Handle(_command, default);
            //Assert
            _amenityRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<Amenity>()), Times.Once);
            result.Should().NotBeNull();
        }
        [Fact]
        public async Task Handler_Should_ThrowsNotFoundException_WhenAmenityIsNull() {
            //Arrange
            _amenityRepositoryMock.Setup(
                x => x.GetByIdAsync(It.IsAny<int>())
            ).ReturnsAsync((Amenity?)null);
            var handler = new DeleteAmenityHandler(_amenityRepositoryMock.Object);
            //Act
            Func<Task> result = async () => await handler.Handle(_command, default);
            //Assert
            _amenityRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<Amenity>()), Times.Never);
            await result.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task Handler_Should_ThrowsException_WhenFail()
        {
            //Arrange
            var handler = new DeleteAmenityHandler(_amenityRepositoryMock.Object);
            _amenityRepositoryMock.Setup(
                x => x.GetByIdAsync(It.IsAny<int>())
            ).ReturnsAsync(new Amenity());
            _amenityRepositoryMock.Setup(x =>
                x.DeleteAsync(
                    It.IsAny<Amenity>()
                )
            ).Throws<Exception>();
            //Act
            Func<Task> result = async () => await handler.Handle(_command, default);
            //Assert
            await result.Should().ThrowAsync<ErrorException>();
        }
    }
}