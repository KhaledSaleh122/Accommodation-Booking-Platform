using Application.CommandsAndQueries.HotelAmenityCQ.Commands.Delete;
using Application.Exceptions;
using Application.Execptions;
using AutoFixture;
using Domain.Abstractions;
using Domain.Entities;
using FluentAssertions;
using Moq;

namespace ABP.Application.Tests.HotelAmenityTests.Commands
{
    public class RemoveAmenityFromHotelHandlerTests
    {
        private readonly Mock<IAmenityRepository> _amenityRepositoryMock;
        private readonly Mock<IHotelAmenityRepository> _hotelAmenityRepositoryMock;
        private readonly Mock<IHotelRepository> _hotelRepositoryMock;
        private readonly IFixture _fixture;
        private readonly RemoveAmenityFromHotelCommand _command;
        private readonly RemoveAmenityFromHotelHandler _handler;
        public RemoveAmenityFromHotelHandlerTests()
        {
            _amenityRepositoryMock = new();
            _hotelRepositoryMock = new();
            _hotelAmenityRepositoryMock = new();
            _fixture = new Fixture();
            _command = new()
            {
                AmenityId = _fixture.Create<int>(),
                HotelId = _fixture.Create<int>(),
            };
            _handler = new(_amenityRepositoryMock.Object, _hotelRepositoryMock.Object,
                _hotelAmenityRepositoryMock.Object);
        }

        [Fact]
        public async Task Handler_Should_RemoveAmenityFromHotel_WhenSuccess()
        {
            //Arrange
            _amenityRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(new Amenity());
            _hotelRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((new Hotel(), default));
            //Act
            await _handler.Handle(_command, default);
            //Assert
            _hotelAmenityRepositoryMock.Verify(x => x.RemoveAmenityAsync(It.IsAny<HotelAmenity>()), Times.Once);
        }

        [Fact]
        public async Task Handler_Should_ThrowNotFoundException_WhenAmenityNotFound()
        {
            //Arrange
            _amenityRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Amenity?)null);
            _hotelRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((new Hotel(), default));
            //Act
            Func<Task> result = async () => await _handler.Handle(_command, default);
            //Assert
            _hotelAmenityRepositoryMock.Verify(x => x.RemoveAmenityAsync(It.IsAny<HotelAmenity>()), Times.Never);
            await result.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task Handler_Should_ThrowNotFoundException_WhenHotelNotFound()
        {
            //Arrange
            _amenityRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(new Amenity());
            _hotelRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(((Hotel, double)?)null);
            //Act
            Func<Task> result = async () => await _handler.Handle(_command, default);
            //Assert
            _hotelAmenityRepositoryMock.Verify(x => x.RemoveAmenityAsync(It.IsAny<HotelAmenity>()), Times.Never);
            await result.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task Handler_Should_ThrowsException_WhenAmenityRepositoryFail()
        {
            //Arrange
            _amenityRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ThrowsAsync(new Exception());
            _hotelRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((new Hotel(), default));
            //Act
            Func<Task> result = async () => await _handler.Handle(_command, default);
            //Assert
            _hotelAmenityRepositoryMock.Verify(x => x.RemoveAmenityAsync(It.IsAny<HotelAmenity>()), Times.Never);
            await result.Should().ThrowAsync<ErrorException>();
        }

        [Fact]
        public async Task Handler_Should_ThrowsException_WhenHotelRepositoryFail()
        {
            //Arrange
            _amenityRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(new Amenity());
            _hotelRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ThrowsAsync(new Exception());
            //Act
            Func<Task> result = async () => await _handler.Handle(_command, default);
            //Assert
            _hotelAmenityRepositoryMock.Verify(x => x.RemoveAmenityAsync(It.IsAny<HotelAmenity>()), Times.Never);
            await result.Should().ThrowAsync<ErrorException>();
        }

        [Fact]
        public async Task Handler_Should_ThrowsException_WhenHotelAmenityRepositoryFail()
        {
            //Arrange
            _amenityRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(new Amenity());
            _hotelRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((new Hotel(), default));
            _hotelAmenityRepositoryMock.Setup(x => x.RemoveAmenityAsync(It.IsAny<HotelAmenity>())).ThrowsAsync(new Exception());
            //Act
            Func<Task> result = async () => await _handler.Handle(_command, default);
            //Assert
            await result.Should().ThrowAsync<ErrorException>();
        }
    }
}
