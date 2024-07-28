using Application.CommandsAndQueries.AmenityCQ.Commands.Update;
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
    public class UpdateAmenityHandlerTests
    {
        private readonly Mock<IAmenityRepository> _amenityRepositoryMock;
        private readonly IFixture _fixture;
        private readonly UpdateAmenityCommand _command;
        private readonly UpdateAmenityHandler _handler;

        public UpdateAmenityHandlerTests()
        {
            _amenityRepositoryMock = new();
            _fixture = new Fixture();
            _command = new UpdateAmenityCommand()
            {
                Description = _fixture.Create<string>(),
                Name = _fixture.Create<string>(),
                id = _fixture.Create<int>()
            };
            _handler = new UpdateAmenityHandler(_amenityRepositoryMock.Object);
        }
        [Fact]
        public async Task Handler_Should_ReturnUpdatedAmenity_WhenSuccess()
        {
            //Arrange
            _amenityRepositoryMock.Setup(
                x => x.GetByIdAsync(It.IsAny<int>())
            ).ReturnsAsync(new Amenity());
            _amenityRepositoryMock.Setup(
                x => x.UpdateAsync(It.IsAny<Amenity>())
            );
            //Act
            AmenityDto result = await _handler.Handle(_command, default);
            //Assert
            _amenityRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Amenity>()), Times.Once);
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
            _amenityRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Amenity>()), Times.Never);
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
                x.UpdateAsync(
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