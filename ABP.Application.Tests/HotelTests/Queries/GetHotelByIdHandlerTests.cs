using Application.CommandsAndQueries.HotelCQ.Query.GetHotelById;
using Application.Dtos.HotelDtos;
using Application.Exceptions;
using Application.Execptions;
using AutoFixture;
using Domain.Abstractions;
using Domain.Entities;
using FluentAssertions;
using Moq;

namespace ABP.Application.Tests.HotelTests.Queries
{
    public class GetHotelByIdHandlerTests
    {
        private readonly Mock<IHotelRepository> _hotelRepositoryMock;
        private readonly Mock<IRecentlyVisitedHotelRepository> _recentlyVisitedHotelRepositoryMock;
        private readonly IFixture _fixture;
        private readonly GetHotelByIdQuery _query;
        private readonly GetHotelByIdHandler _handler;
        public GetHotelByIdHandlerTests()
        {
            _hotelRepositoryMock = new();
            _recentlyVisitedHotelRepositoryMock = new();
            _fixture = new Fixture();
            _query = new GetHotelByIdQuery()
            {
                HotelId = _fixture.Create<int>()
            };
            _handler = new GetHotelByIdHandler(_hotelRepositoryMock.Object, _recentlyVisitedHotelRepositoryMock.Object);
        }
        [Fact]
        public async Task Handler_Should_ReturnRequestedHotel_WhenSuccess()
        {
            //Arrange
            _hotelRepositoryMock.Setup(
                x => x.GetByIdAsync(It.IsAny<int>())
            ).ReturnsAsync((new Hotel(),default));
            //Act
            HotelFullDto result = (await _handler.Handle(_query, default))!;
            //Assert
            _hotelRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
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
            Func<Task> result = async () => await _handler.Handle(_query, default);
            //Assert
            _hotelRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Never);
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
                x.GetByIdAsync(
                    It.IsAny<int>()
                )
            ).Throws<Exception>();
            //Act
            Func<Task> result = async () => await _handler.Handle(_query, default);
            //Assert
            await result.Should().ThrowAsync<ErrorException>();
        }
    }
}