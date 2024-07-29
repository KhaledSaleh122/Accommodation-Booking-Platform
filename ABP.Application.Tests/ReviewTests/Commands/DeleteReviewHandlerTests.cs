using Application.CommandsAndQueries.ReviewCQ.Commands.Delete;
using Application.Dtos.ReviewDtos;
using Application.Exceptions;
using Application.Execptions;
using AutoFixture;
using Domain.Abstractions;
using Domain.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace ABP.Application.Tests.ReviewTests.Commands
{
    public class DeleteReviewHandlerTests
    {
        private readonly Mock<IReviewHotelRepository> _reviewRepositoryMock;
        private readonly Mock<IHotelRepository> _hotelRepositoryMock;
        private readonly DeleteReviewHandler _handler;
        private readonly Mock<UserManager<User>> _userManagerMock;
        private readonly IFixture _fixture;
        private readonly DeleteReviewCommand _command;
        public DeleteReviewHandlerTests()
        {
            var store = new Mock<IUserStore<User>>();
            _userManagerMock = new Mock<UserManager<User>>(
                store.Object, null, null, null, null, null, null, null, null);
            _reviewRepositoryMock = new();
            _hotelRepositoryMock = new();
            _fixture = new Fixture();
            _command = new DeleteReviewCommand()
            {
                HotelId = _fixture.Create<int>(),
                UserId = _fixture.Create<string>()
            };
            _handler = new DeleteReviewHandler(
                _reviewRepositoryMock.Object, _hotelRepositoryMock.Object, _userManagerMock.Object);
        }
        [Fact]
        public async Task Handler_Should_ReturnDeletedReview_WhenSuccess()
        {
            //Arrange
            _hotelRepositoryMock.Setup(
                x => x.GetByIdAsync(It.IsAny<int>())
            ).ReturnsAsync((new Hotel(),default));
            _userManagerMock.Setup(
                x => x.FindByIdAsync(It.IsAny<string>())
            ).ReturnsAsync(new User());
            _reviewRepositoryMock.Setup(
                x => x.GetReviewAsync(It.IsAny<int>(), It.IsAny<string>())
            ).ReturnsAsync(new Review());
            _reviewRepositoryMock.Setup(
                x => x.DeleteHotelReviewAsync(It.IsAny<Review>())
            ).ReturnsAsync(new Review());
            //Act
            ReviewDto result = await _handler.Handle(_command, default);
            //Assert
            _reviewRepositoryMock.Verify(x => x.DeleteHotelReviewAsync(It.IsAny<Review>()), Times.Once);
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task Handler_Should_ThrowsNotFoundException_WhenReviewIsNull()
        {
            //Arrange
            _hotelRepositoryMock.Setup(
                x => x.GetByIdAsync(It.IsAny<int>())
            ).ReturnsAsync((new Hotel(), default));
            _userManagerMock.Setup(
                x => x.FindByIdAsync(It.IsAny<string>())
            ).ReturnsAsync(new User());
            _reviewRepositoryMock.Setup(
                x => x.GetReviewAsync(It.IsAny<int>(),It.IsAny<string>())
            ).ReturnsAsync((Review?)null);
            //Act
            Func<Task> result = async () => await _handler.Handle(_command, default);
            //Assert
            _reviewRepositoryMock.Verify(x => x.DeleteHotelReviewAsync(It.IsAny<Review>()), Times.Never);
            await result.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task Handler_Should_ThrowsNotFoundException_WhenHotelIsNull()
        {
            //Arrange
            _userManagerMock.Setup(
                x => x.FindByIdAsync(It.IsAny<string>())
            ).ReturnsAsync(new User());
            _reviewRepositoryMock.Setup(
                x => x.GetReviewAsync(It.IsAny<int>(), It.IsAny<string>())
            ).ReturnsAsync(new Review());
            _hotelRepositoryMock.Setup(
                x => x.GetByIdAsync(It.IsAny<int>())
            ).ReturnsAsync(((Hotel,double)?) null);
            //Act
            Func<Task> result = async () => await _handler.Handle(_command, default);
            //Assert
            _reviewRepositoryMock.Verify(x => x.DeleteHotelReviewAsync(It.IsAny<Review>()), Times.Never);
            await result.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task Handler_Should_ThrowsNotFoundException_WhenUserIsNull()
        {
            //Arrange

            _hotelRepositoryMock.Setup(
                x => x.GetByIdAsync(It.IsAny<int>())
            ).ReturnsAsync((new Hotel(), default));
            _userManagerMock.Setup(
                x => x.FindByIdAsync(It.IsAny<string>())
            ).ReturnsAsync((User?)null);
            _reviewRepositoryMock.Setup(
                x => x.GetReviewAsync(It.IsAny<int>(), It.IsAny<string>())
            ).ReturnsAsync(new Review());
            //Act
            Func<Task> result = async () => await _handler.Handle(_command, default);
            //Assert
            _reviewRepositoryMock.Verify(x => x.DeleteHotelReviewAsync(It.IsAny<Review>()), Times.Never);
            await result.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task Handler_Should_ThrowsException_WhenFail()
        {
            //Arrange
            _hotelRepositoryMock.Setup(
                x => x.GetByIdAsync(It.IsAny<int>())
            ).ReturnsAsync((new Hotel(), default));
            _userManagerMock.Setup(
                x => x.FindByIdAsync(It.IsAny<string>())
            ).ReturnsAsync(new User());
            _reviewRepositoryMock.Setup(
                x => x.GetReviewAsync(It.IsAny<int>(), It.IsAny<string>())
            ).ReturnsAsync(new Review());
            _reviewRepositoryMock.Setup(x =>
                x.DeleteHotelReviewAsync(
                    It.IsAny<Review>()
                )
            ).Throws<Exception>();
            //Act
            Func<Task> result = async () => await _handler.Handle(_command, default);
            //Assert
            await result.Should().ThrowAsync<ErrorException>();
        }
    }
}