using Application.CommandsAndQueries.HotelCQ.Commands.Delete;
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
    public class DeleteHotelHandlerTests
    {
        private readonly Mock<IHotelRepository> _hotelRepositoryMock;
        private readonly Mock<IImageService> _imageRepositoryMock;
        private readonly Mock<ITransactionService> _transactionServiceMock;
        private readonly DeleteHotelHandler _handler;
        private readonly IFixture _fixture;
        private readonly DeleteHotelCommand _command;
        public DeleteHotelHandlerTests()
        {
            _hotelRepositoryMock = new();
            _imageRepositoryMock = new();
            _transactionServiceMock = new();
            _fixture = new Fixture();
            _command = new DeleteHotelCommand()
            {
                Id = _fixture.Create<int>()
            };
            _handler = new DeleteHotelHandler(
                _hotelRepositoryMock.Object, _imageRepositoryMock.Object, _transactionServiceMock.Object);
        }
        [Fact]
        public async Task Handler_Should_ReturnDeletedHotel_WhenSuccess()
        {
            //Arrange
            _hotelRepositoryMock.Setup(
                x => x.GetByIdAsync(It.IsAny<int>())
            ).ReturnsAsync((new Hotel() { Images = [] }, default));
            _hotelRepositoryMock.Setup(
                x => x.DeleteAsync(It.IsAny<Hotel>())
            ).ReturnsAsync(new Hotel());
            //Act
            HotelMinDto result = await _handler.Handle(_command, default);
            //Assert
            _hotelRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<Hotel>()), Times.Once);
            result.Should().NotBeNull();
        }
        [Fact]
        public async Task Handler_Should_ThrowsNotFoundException_WhenHotelIsNull()
        {
            //Arrange
            _hotelRepositoryMock.Setup(
                x => x.GetByIdAsync(It.IsAny<int>())
            ).ReturnsAsync(((Hotel, double)?)null);
            //Act
            Func<Task> result = async () => await _handler.Handle(_command, default);
            //Assert
            _hotelRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<Hotel>()), Times.Never);
            await result.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task Handler_Should_ThrowsException_WhenFail()
        {
            //Arrange
            _hotelRepositoryMock.Setup(
                x => x.GetByIdAsync(It.IsAny<int>())
            ).ReturnsAsync((new Hotel(), default));
            _hotelRepositoryMock.Setup(x =>
                x.DeleteAsync(
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