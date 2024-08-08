using Application.CommandsAndQueries.ReviewCQ.Commands.Create;
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
    public class CreateReviewHandlerTests
    {
        private readonly Mock<IReviewHotelRepository> _reviewRepositoryMock;
        private readonly Mock<IHotelRepository> _hotelRepositoryMock;
        private readonly CreateReviewHandler _handler;
        private readonly Mock<UserManager<User>> _userManagerMock;
        private readonly IFixture _fixture;
        private readonly CreateReviewCommand _command;
        public CreateReviewHandlerTests()
        {
            _reviewRepositoryMock = new();
            _hotelRepositoryMock = new();
            var store = new Mock<IUserStore<User>>();
            _userManagerMock = new Mock<UserManager<User>>(
                store.Object, null, null, null, null, null, null, null, null);
            _fixture = new Fixture();
            _command = new CreateReviewCommand()
            {
                Comment = _fixture.Create<string>(),
                hotelId = _fixture.Create<int>(),
                Rating = _fixture.Create<int>(),
                userId = _fixture.Create<string>()
            };
            _handler = new CreateReviewHandler(
                _reviewRepositoryMock.Object, _hotelRepositoryMock.Object, _userManagerMock.Object);
        }
        [Fact]
        public async Task Handler_Should_ReturnCreatedReview_WhenSuccess()
        {
            //Act
            _hotelRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((new Hotel(), default));
            _reviewRepositoryMock.Setup(x => x.DoesUserReviewedAsync(It.IsAny<int>(), It.IsAny<string>())).ReturnsAsync(false);
            _reviewRepositoryMock.Setup(x => x.AddHotelReviewAsync(It.IsAny<Review>())).ReturnsAsync(new Review());
            ReviewDto result = await _handler.Handle(_command, default);
            //Assert
            _reviewRepositoryMock.Verify(x => x.AddHotelReviewAsync(It.IsAny<Review>()), Times.Once);
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task Handler_Should_ThrowsErrorException_WhenUserAlreadyMadeAReviewToTheHotel()
        {
            //Arrange
            _hotelRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((new Hotel(), default));
            _reviewRepositoryMock.Setup(x => x.DoesUserReviewedAsync(It.IsAny<int>(), It.IsAny<string>())).ReturnsAsync(true);
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
            _reviewRepositoryMock.Verify(x => x.AddHotelReviewAsync(It.IsAny<Review>()), Times.Never);
            await result.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task Handler_Should_ThrowsException_WhenFail()
        {
            //Arrange
            _hotelRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((new Hotel(), default));
            _reviewRepositoryMock.Setup(x => x.DoesUserReviewedAsync(It.IsAny<int>(), It.IsAny<string>())).ReturnsAsync(false);
            _reviewRepositoryMock.Setup(x =>
                x.AddHotelReviewAsync(
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