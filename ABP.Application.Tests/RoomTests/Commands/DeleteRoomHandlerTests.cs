using Application.CommandsAndQueries.RoomCQ.Commands.Delete;
using Application.Dtos.RoomDtos;
using Application.Exceptions;
using Application.Execptions;
using AutoFixture;
using Domain.Abstractions;
using Domain.Entities;
using FluentAssertions;
using Moq;

namespace ABP.Application.Tests.RoomTests.Commands
{
    public class DeleteRoomHandlerTests
    {
        private readonly Mock<IHotelRoomRepository> _roomRepositoryMock;
        private readonly Mock<IHotelRepository> _hotelRepositoryMock;
        private readonly Mock<IImageService> _imageRepositoryMock;
        private readonly Mock<ITransactionService> _transactionServiceMock;
        private readonly DeleteRoomHandler _handler;
        private readonly IFixture _fixture;
        private readonly DeleteRoomCommand _command;
        public DeleteRoomHandlerTests()
        {
            _imageRepositoryMock = new();
            _transactionServiceMock = new();
            _roomRepositoryMock = new();
            _hotelRepositoryMock = new();
            _fixture = new Fixture();
            _command = new DeleteRoomCommand()
            {
                HotelId = _fixture.Create<int>(),
                RoomNumber = _fixture.Create<string>(),
            };
            _handler = new DeleteRoomHandler(
                _hotelRepositoryMock.Object, _imageRepositoryMock.Object, _roomRepositoryMock.Object, _transactionServiceMock.Object);
        }
        [Fact]
        public async Task Handler_Should_ReturnDeletedRoom_WhenSuccess()
        {
            //Arrange
            _hotelRepositoryMock.Setup(
                x => x.GetByIdAsync(It.IsAny<int>())
            ).ReturnsAsync((new Hotel(), default));
            _roomRepositoryMock.Setup(
                x => x.GetHotelRoomAsync(It.IsAny<int>(), It.IsAny<string>())
            ).ReturnsAsync(new Room() { Images = [] });
            _roomRepositoryMock.Setup(
                x => x.DeleteRoomAsync(It.IsAny<Room>())
            ).ReturnsAsync(new Room());
            //Act
            RoomDto result = await _handler.Handle(_command, default);
            //Assert
            _roomRepositoryMock.Verify(x => x.DeleteRoomAsync(It.IsAny<Room>()), Times.Once);
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task Handler_Should_ThrowsNotFoundException_WhenRoomIsNull()
        {
            //Arrange
            _hotelRepositoryMock.Setup(
                x => x.GetByIdAsync(It.IsAny<int>())
            ).ReturnsAsync((new Hotel(), default));
            _roomRepositoryMock.Setup(
                x => x.GetHotelRoomAsync(It.IsAny<int>(), It.IsAny<string>())
            ).ReturnsAsync((Room?)null);
            //Act
            Func<Task> result = async () => await _handler.Handle(_command, default);
            //Assert
            _roomRepositoryMock.Verify(x => x.DeleteRoomAsync(It.IsAny<Room>()), Times.Never);
            await result.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task Handler_Should_ThrowsNotFoundException_WhenHotelIsNull()
        {
            //Arrange
            _roomRepositoryMock.Setup(
                x => x.GetHotelRoomAsync(It.IsAny<int>(), It.IsAny<string>())
            ).ReturnsAsync(new Room());
            _hotelRepositoryMock.Setup(
                x => x.GetByIdAsync(It.IsAny<int>())
            ).ReturnsAsync(((Hotel, double)?)null);
            //Act
            Func<Task> result = async () => await _handler.Handle(_command, default);
            //Assert
            _roomRepositoryMock.Verify(x => x.DeleteRoomAsync(It.IsAny<Room>()), Times.Never);
            await result.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task Handler_Should_ThrowsException_WhenFail()
        {
            //Arrange
            _hotelRepositoryMock.Setup(
                x => x.GetByIdAsync(It.IsAny<int>())
            ).ReturnsAsync((new Hotel(), default));
            _roomRepositoryMock.Setup(
                x => x.GetHotelRoomAsync(It.IsAny<int>(), It.IsAny<string>())
            ).ReturnsAsync(new Room());
            _roomRepositoryMock.Setup(x =>
                x.DeleteRoomAsync(
                    It.IsAny<Room>()
                )
            ).Throws<Exception>();
            //Act
            Func<Task> result = async () => await _handler.Handle(_command, default);
            //Assert
            await result.Should().ThrowAsync<ErrorException>();
        }
    }
}