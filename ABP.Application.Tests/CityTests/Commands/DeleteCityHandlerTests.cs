using Application.CommandsAndQueries.CityCQ.Commands.Delete;
using Application.Dtos.CityDtos;
using Application.Exceptions;
using Application.Execptions;
using AutoFixture;
using Domain.Abstractions;
using Domain.Entities;
using FluentAssertions;
using Moq;

namespace ABP.Application.Tests.CityTests.Commands
{
    public class DeleteCityHandlerTests
    {
        private readonly Mock<ICityRepository> _cityRepositoryMock;
        private readonly Mock<IImageService> _imageRepositoryMock;
        private readonly Mock<ITransactionService> _transactionServiceMock;
        private readonly DeleteCityHandler _handler;
        private readonly IFixture _fixture;
        private readonly DeleteCityCommand _command;
        public DeleteCityHandlerTests()
        {
            _cityRepositoryMock = new();
            _imageRepositoryMock = new();
            _transactionServiceMock = new();
            _fixture = new Fixture();
            _command = new DeleteCityCommand()
            {
                Id = _fixture.Create<int>()
            };
            _handler = new DeleteCityHandler(
                _cityRepositoryMock.Object, _imageRepositoryMock.Object, _transactionServiceMock.Object);
        }
        [Fact]
        public async Task Handler_Should_ReturnDeletedCity_WhenSuccess()
        {
            //Arrange
            _cityRepositoryMock.Setup(
                x => x.GetByIdAsync(It.IsAny<int>())
            ).ReturnsAsync(new City());
            _cityRepositoryMock.Setup(
                x => x.DeleteAsync(It.IsAny<City>())
            ).ReturnsAsync(new City());
            //Act
            CityDto result = await _handler.Handle(_command, default);
            //Assert
            _cityRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<City>()), Times.Once);
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
            Func<Task> result = async () => await _handler.Handle(_command, default);
            //Assert
            _cityRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<City>()), Times.Never);
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
                x.DeleteAsync(
                    It.IsAny<City>()
                )
            ).Throws<Exception>();
            //Act
            Func<Task> result = async () => await _handler.Handle(_command, default);
            //Assert
            await result.Should().ThrowAsync<ErrorException>();
        }
    }
}