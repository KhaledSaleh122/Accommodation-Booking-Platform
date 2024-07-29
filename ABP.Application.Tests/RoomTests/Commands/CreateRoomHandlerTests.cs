using Application.CommandsAndQueries.RoomCQ.Commands.Create;
using Application.Dtos.RoomDtos;
using Application.Exceptions;
using Application.Execptions;
using AutoFixture;
using Domain.Abstractions;
using Domain.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Text;

namespace ABP.Application.Tests.RoomTests.Commands
{
    public class CreateRoomHandlerTests
    {
        private readonly Mock<IHotelRoomRepository> _roomRepositoryMock;
        private readonly Mock<IHotelRepository> _hotelRepositoryMock;
        private readonly Mock<IImageService> _imageRepositoryMock;
        private readonly Mock<ITransactionService> _transactionServiceMock;
        private readonly CreateRoomHandler _handler;
        private readonly IFixture _fixture;
        private readonly CreateRoomCommand _command;
        public CreateRoomHandlerTests()
        {
            _imageRepositoryMock = new();
            _transactionServiceMock = new();
            _roomRepositoryMock = new();
            _hotelRepositoryMock = new();
            _fixture = new Fixture();
            var bytes = Encoding.UTF8.GetBytes("This is a dummy file");
            IFormFile testFile = new FormFile(new MemoryStream(bytes), 0, bytes.Length, "dummy", "dummy.png");
            _command = new CreateRoomCommand()
            {
                AdultCapacity = _fixture.Create<int>(),
                ChildrenCapacity = _fixture.Create<int>(),
                hotelId = _fixture.Create<int>(),
                RoomNumber = _fixture.Create<string>(),
                Thumbnail = testFile,
                Images = [testFile]
            };
            _handler = new CreateRoomHandler(
                _hotelRepositoryMock.Object, _imageRepositoryMock.Object,_transactionServiceMock.Object,_roomRepositoryMock.Object);
        }
        [Fact]
        public async Task Handler_Should_ReturnCreatedRoom_WhenSuccess()
        {
            //Act
            _hotelRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((new Hotel(), default));
            _roomRepositoryMock.Setup(x => x.RoomNumberExistsAsync(It.IsAny<int>(), It.IsAny<string>())).ReturnsAsync(false);
            RoomDto result = await _handler.Handle(_command, default);
            //Assert
            _roomRepositoryMock.Verify(x => x.AddRoomAsync(It.IsAny<Room>()), Times.Once);
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task Handler_Should_ThrowsErrorException_WhenThereAlreadyRoomWithTheRoomNumber()
        {
            //Arrange
            _hotelRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((new Hotel(), default));
            _roomRepositoryMock.Setup(x => x.RoomNumberExistsAsync(It.IsAny<int>(), It.IsAny<string>())).ReturnsAsync(true);
            //Act
            Func<Task> result = async () => await _handler.Handle(_command, default);
            //Assert
            await result.Should().ThrowAsync<ErrorException>();
        }

        [Fact]
        public async Task Handler_Should_ThrowsNotFoundException_WhenHotelIsNull()
        {
            //Arrange
            _hotelRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(((Hotel, double)?)null);
            //Act
            Func<Task> result = async () => await _handler.Handle(_command, default);
            //Assert
            _roomRepositoryMock.Verify(x => x.AddRoomAsync(It.IsAny<Room>()), Times.Never);
            await result.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task Handler_Should_ThrowsException_WhenFail()
        {
            //Arrange
            _hotelRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((new Hotel(), default));
            _roomRepositoryMock.Setup(x => x.RoomNumberExistsAsync(It.IsAny<int>(), It.IsAny<string>())).ReturnsAsync(false);
            _roomRepositoryMock.Setup(x =>
                x.AddRoomAsync(
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