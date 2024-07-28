using Application.CommandsAndQueries.AmenityCQ.Commands.Create;
using Application.Dtos.AmenityDtos;
using Application.Execptions;
using AutoFixture;
using Domain.Abstractions;
using Domain.Entities;
using FluentAssertions;
using Moq;

namespace ABP.Application.Tests
{
    public class CreateAmenityHandlerTests
    {
        private readonly Mock<IAmenityRepository> _amenityRepositoryMock;
        private readonly IFixture _fixture;
        private readonly CreateAmenityCommand _command;
        public CreateAmenityHandlerTests() {
            _amenityRepositoryMock = new();
            _fixture = new Fixture();
            _command = new CreateAmenityCommand()
            {
                Description = _fixture.Create<string>(),
                Name = _fixture.Create<string>()
            };
        }
        [Fact]
        public async Task Handler_Should_ReturnCreatedAmenity_WhenSuccess()
        {
            //Arrange
            var handler = new CreateAmenityHandler(_amenityRepositoryMock.Object);
            //Act
            AmenityDto result = await handler.Handle(_command, default);
            //Assert
            _amenityRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Amenity>()), Times.Once);
            result.Should().NotBeNull();
        }
        [Fact]
        public async Task Handler_Should_ThrowsException_WhenFail() {
            //Arrange
            var handler = new CreateAmenityHandler(_amenityRepositoryMock.Object);
            _amenityRepositoryMock.Setup(x =>
                x.CreateAsync(
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