using Application.CommandsAndQueries.RecentlyVisitedHotelCQ.Queries.GetRecentlyVisitedHotels;
using Application.Dtos.RecentlyVisitedHotelDto;
using Application.Exceptions;
using Application.Execptions;
using AutoFixture;
using Domain.Abstractions;
using Domain.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace ABP.Application.Tests.RecentlyVisitedHotelTests.Queries
{
    public class GetRecentlyVisitedHotelHandlerTests
    {
        private readonly Mock<IRecentlyVisitedHotelRepository> _RecentlyVisitedHotelRepositoryMock;
        private readonly Mock<UserManager<User>> _userManagerMock;
        private readonly IFixture _fixture;
        private readonly GetRvhCommand _query;
        private readonly GetRvhHandler _handler;

        public GetRecentlyVisitedHotelHandlerTests()
        {
            var store = new Mock<IUserStore<User>>();
            _userManagerMock = new Mock<UserManager<User>>(
                store.Object, null, null, null, null, null, null, null, null);

            _RecentlyVisitedHotelRepositoryMock = new Mock<IRecentlyVisitedHotelRepository>();
            _fixture = new Fixture();
            int page = _fixture.Create<int>();
            int pageSize = _fixture.Create<int>();
            _query = new GetRvhCommand { UserId = _fixture.Create<string>() };
            _handler = new GetRvhHandler(_RecentlyVisitedHotelRepositoryMock.Object, _userManagerMock.Object);
        }

        [Fact]
        public async Task Handler_Should_ReturnRecentlyVisitedHotel_WhenSuccess()
        {
            // Arrange
            _RecentlyVisitedHotelRepositoryMock.Setup(x =>
                x.GetAsync(It.IsAny<string>())
            ).ReturnsAsync(new Dictionary<RecentlyVisitedHotel, double>());
            _userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(new User());

            // Act
            IEnumerable<RvhDto> result = await _handler.Handle(_query, default);

            // Assert
            _RecentlyVisitedHotelRepositoryMock.Verify(
                x => x.GetAsync(It.IsAny<string>()),
                Times.Once
            );
            result.Should().NotBeNull();
        }
        [Fact]
        public async Task Handler_Should_ThrowsNotFoundException_WhenUserIsNull()
        {
            //Arrange
            _RecentlyVisitedHotelRepositoryMock.Setup(x =>
                x.GetAsync(It.IsAny<string>())
            ).ReturnsAsync(new Dictionary<RecentlyVisitedHotel, double>());
            //Act
            Func<Task> result = async () => await _handler.Handle(_query, default);
            //Assert
            _RecentlyVisitedHotelRepositoryMock.Verify(x => x.GetAsync(It.IsAny<string>()), Times.Never);
            await result.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task Handler_Should_ThrowsException_WhenFail()
        {
            // Arrange
            _RecentlyVisitedHotelRepositoryMock.Setup(x =>
                x.GetAsync(It.IsAny<string>())
            ).Throws<Exception>();
            _userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(new User());
            // Act
            Func<Task> result = async () => await _handler.Handle(_query, default);

            // Assert
            await result.Should().ThrowAsync<ErrorException>();
        }
    }
}
