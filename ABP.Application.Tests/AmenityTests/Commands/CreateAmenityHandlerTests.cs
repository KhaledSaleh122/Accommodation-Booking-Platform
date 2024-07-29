using Application.CommandsAndQueries.AmenityCQ.Commands.Create;
using Application.Dtos.AmenityDtos;
using Application.Execptions;
using AutoFixture;
using Domain.Abstractions;
using Domain.Entities;
using FluentAssertions;
using Moq;

namespace ABP.Application.Tests.AmenityTests.Commands
{
    public class CreateAmenityHandlerTests
    {
        private readonly Mock<IAmenityRepository> _amenityRepositoryMock;
        private readonly IFixture _fixture;
        private readonly CreateAmenityCommand _command;
        private readonly CreateAmenityHandler _handler;
        public CreateAmenityHandlerTests()
        {
            _amenityRepositoryMock = new();
            _fixture = new Fixture();
            _command = new CreateAmenityCommand()
            {
                Description = _fixture.Create<string>(),
                Name = _fixture.Create<string>()
            };
            _handler = new CreateAmenityHandler(_amenityRepositoryMock.Object);
        }
        [Fact]
        public async Task Handler_Should_ReturnCreatedAmenity_WhenSuccess()
        {
            //Arrange
            //Act
            AmenityDto result = await _handler.Handle(_command, default);
            //Assert
            _amenityRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Amenity>()), Times.Once);
            result.Should().NotBeNull();
        }
        [Fact]
        public async Task Handler_Should_ThrowsException_WhenFail()
        {
            //Arrange
            _amenityRepositoryMock.Setup(x =>
                x.CreateAsync(
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