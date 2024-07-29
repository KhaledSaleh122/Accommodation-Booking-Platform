using Application.CommandsAndQueries.CityCQ.Commands.Update;
using Application.CommandsAndQueries.HotelCQ.Commands.Update;
using Application.Dtos.HotelDtos;
using Application.Exceptions;
using Application.Execptions;
using AutoFixture;
using Domain.Abstractions;
using Domain.Entities;
using FluentAssertions;
using Moq;

namespace ABP.Application.Tests.HotelTests.Commands
{
    public class UpdateHotelHandlerTests
    {
        private readonly Mock<IHotelRepository> _hotelRepositoryMock;
        private readonly IFixture _fixture;
        private readonly UpdateHotelCommand _command;
        private readonly UpdateHotelHandler _handler;
        public UpdateHotelHandlerTests()
        {
            _hotelRepositoryMock = new();
            _fixture = new Fixture();
            _command = new UpdateHotelCommand()
            {
                Address = _fixture.Create<string>(),
                Description = _fixture.Create<string>(),
                HotelType = _fixture.Create<int>(),
                Name = _fixture.Create<string>(),
                Owner = _fixture.Create<string>(),
                PricePerNight = _fixture.Create<int>(),
                hotelId = _fixture.Create<int>()
            };
            _handler = new UpdateHotelHandler(_hotelRepositoryMock.Object);
        }
        [Fact]
        public async Task Handler_Should_ReturnUpdatedHotel_WhenSuccess()
        {
            //Arrange
            _hotelRepositoryMock.Setup(
                x => x.GetByIdAsync(It.IsAny<int>())
            ).ReturnsAsync((new Hotel(),default));
            _hotelRepositoryMock.Setup(
                x => x.UpdateAsync(It.IsAny<Hotel>())
            );
            //Act
            HotelMinDto result = await _handler.Handle(_command, default);
            //Assert
            _hotelRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Hotel>()), Times.Once);
            result.Should().NotBeNull();
        }
        [Fact]
        public async Task Handler_Should_ThrowsNotFoundException_WhenHotelIsNull()
        {
            //Arrange
            _hotelRepositoryMock.Setup(
                x => x.GetByIdAsync(It.IsAny<int>())
            ).ReturnsAsync(((Hotel,double)?)null);
            //Act
            Func<Task> result = async () => await _handler.Handle(_command, default);
            //Assert
            _hotelRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Hotel>()), Times.Never);
            await result.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task Handler_Should_ThrowsException_WhenFail()
        {
            //Arrange
            _hotelRepositoryMock.Setup(
                x => x.GetByIdAsync(It.IsAny<int>())
            ).ReturnsAsync((new Hotel(),default));
            _hotelRepositoryMock.Setup(x =>
                x.UpdateAsync(
                    It.IsAny<Hotel>()
                )
            ).Throws<Exception>();
            //Act
            Func<Task> result = async () => await _handler.Handle(_command, default);
            //Assert
            await result.Should().ThrowAsync<ErrorException>();
        }
    }
}