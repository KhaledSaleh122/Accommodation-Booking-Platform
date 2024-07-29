using Application.CommandsAndQueries.AmenityCQ.Commands.Delete;
using Application.Dtos.AmenityDtos;
using Application.Exceptions;
using Application.Execptions;
using AutoFixture;
using Domain.Abstractions;
using Domain.Entities;
using FluentAssertions;
using Moq;

namespace ABP.Application.Tests.AmenityTests.Commands
{
    public class DeleteAmenityHandlerTests
    {
        private readonly Mock<IAmenityRepository> _amenityRepositoryMock;
        private readonly IFixture _fixture;
        private readonly DeleteAmenityCommand _command;
        private readonly DeleteAmenityHandler _handler;

        public DeleteAmenityHandlerTests()
        {
            _amenityRepositoryMock = new();
            _fixture = new Fixture();
            _command = new DeleteAmenityCommand()
            {
                Id = _fixture.Create<int>()
            };
            _handler = new DeleteAmenityHandler(_amenityRepositoryMock.Object);
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
            //Act
            AmenityDto result = await _handler.Handle(_command, default);
            //Assert
            _amenityRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<Amenity>()), Times.Once);
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
            Func<Task> result = async () => await _handler.Handle(_command, default);
            //Assert
            _amenityRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<Amenity>()), Times.Never);
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
                x.DeleteAsync(
                    It.IsAny<Amenity>()
                )
            ).Throws<Exception>();
            //Act
            Func<Task> result = async () => await _handler.Handle(_command, default);
            //Assert
            await result.Should().ThrowAsync<ErrorException>();
        }
    }
}