using Application.CommandsAndQueries.CityCQ.Query.GetCityById;
using Application.Dtos.CityDtos;
using Application.Exceptions;
using Application.Execptions;
using AutoFixture;
using Domain.Abstractions;
using Domain.Entities;
using FluentAssertions;
using Moq;

namespace ABP.Application.Tests
{
    public class GetCityByIdHandlerTests
    {
        private readonly Mock<ICityRepository> _cityRepositoryMock;
        private readonly IFixture _fixture;
        private readonly GetCityByIdQuery _query;
        private readonly GetCityByIdHandler _handler;
        public GetCityByIdHandlerTests()
        {
            _cityRepositoryMock = new();
            _fixture = new Fixture();
            _query = new GetCityByIdQuery()
            {
                CityId = _fixture.Create<int>()
            };
            _handler = new GetCityByIdHandler(_cityRepositoryMock.Object);
        }
        [Fact]
        public async Task Handler_Should_ReturnRequestedCity_WhenSuccess()
        {
            //Arrange
            _cityRepositoryMock.Setup(
                x => x.GetByIdAsync(It.IsAny<int>())
            ).ReturnsAsync(new City());
            //Act
            CityDto result = (await _handler.Handle(_query, default))!;
            //Assert
            _cityRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
            result.Should().NotBeNull();
        }
        [Fact]
        public async Task Handler_Should_ThrowsNotFoundException_WhenCityIsNull()
        {
            //Arrange
            _cityRepositoryMock.Setup(
                x => x.GetByIdAsync(It.IsAny<int>())
            ).ReturnsAsync((City?)null);
            //Act
            Func<Task> result = async () => await _handler.Handle(_query, default);
            //Assert
            _cityRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Never);
            await result.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task Handler_Should_ThrowsException_WhenFail()
        {
            //Arrange
            _cityRepositoryMock.Setup(
                x => x.GetByIdAsync(It.IsAny<int>())
            ).ReturnsAsync(new City());
            _cityRepositoryMock.Setup(x =>
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