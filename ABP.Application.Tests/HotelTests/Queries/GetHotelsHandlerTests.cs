using Application.CommandsAndQueries.HotelCQ.Query.GetHotels;
using Application.Dtos.HotelDtos;
using Application.Execptions;
using AutoFixture;
using Domain.Abstractions;
using Domain.Entities;
using Domain.Params;
using FluentAssertions;
using Moq;

namespace ABP.Application.Tests.HotelTests.Queries
{
    public class GetHotelsHandlerTests
    {
        private readonly Mock<IHotelRepository> _hotelRepositoryMock;
        private readonly IFixture _fixture;
        private readonly GetHotelsQuery _query;
        private readonly GetHotelsHandler _handler;
        public GetHotelsHandlerTests()
        {
            _hotelRepositoryMock = new();
            _fixture = new Fixture();
            int page = _fixture.Create<int>();
            int pageSize = _fixture.Create<int>();
            _query = new(page, pageSize);
            _handler = new GetHotelsHandler(_hotelRepositoryMock.Object);
        }
        [Fact]
        public async Task Handler_Should_ReturnHotels_WhenSuccess()
        {
            //Arrange
            _hotelRepositoryMock.Setup(x =>
                x.GetAsync(It.IsAny<HotelSearch>())
            ).ReturnsAsync((new Dictionary<Hotel,double>(), default));
            //Act
            (IEnumerable<HotelDto> result, int totalRecords, int page, int pageSize) =
                await _handler.Handle(_query, default);
            //Assert
            _hotelRepositoryMock.Verify(
                x =>
                    x.GetAsync(It.IsAny<HotelSearch>()),
               Times.Once
            );
            result.Should().NotBeNull();
            totalRecords.Should().BeGreaterThanOrEqualTo(0);
            page.Should().BeGreaterThanOrEqualTo(1);
            pageSize.Should().BeInRange(1, 100);
        }
        [Fact]
        public async Task Handler_Should_ThrowsException_WhenFail()
        {
            //Arrange
            _hotelRepositoryMock.Setup(x =>
                x.GetAsync(It.IsAny<HotelSearch>())
            ).Throws<Exception>();
            //Act
            Func<Task> result = async () => await _handler.Handle(_query, default);
            //Assert
            await result.Should().ThrowAsync<ErrorException>();
        }
    }
}