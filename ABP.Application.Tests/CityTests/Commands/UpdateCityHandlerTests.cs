using Application.CommandsAndQueries.CityCQ.Commands.Update;
using Application.Dtos.CityDtos;
using Application.Exceptions;
using Application.Execptions;
using AutoFixture;
using Domain.Abstractions;
using Domain.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Text;

namespace ABP.Application.Tests.CityTests.Commands
{
    public class UpdateCityHandlerTests
    {
        private readonly Mock<ICityRepository> _cityRepositoryMock;
        private readonly Mock<IImageService> _imageRepositoryMock;
        private readonly Mock<ITransactionService> _transactionServiceMock;
        private readonly IFixture _fixture;
        private readonly UpdateCityCommand _command;
        private readonly UpdateCityHandler _handler;
        public UpdateCityHandlerTests()
        {
            _cityRepositoryMock = new();
            _imageRepositoryMock = new();
            _transactionServiceMock = new();
            _fixture = new Fixture();
            var bytes = Encoding.UTF8.GetBytes("This is a dummy file");
            IFormFile testFile = new FormFile(new MemoryStream(bytes), 0, bytes.Length, "dummy", "dummy.png");
            _command = new UpdateCityCommand()
            {
                id = _fixture.Create<int>(),
                Country = _fixture.Create<string>(),
                Name = _fixture.Create<string>(),
                PostOffice = _fixture.Create<string>(),
                Thumbnail = testFile

            };
            _handler = new UpdateCityHandler(_cityRepositoryMock.Object, _imageRepositoryMock.Object,_transactionServiceMock.Object);
        }
        [Fact]
        public async Task Handler_Should_ReturnUpdatedCity_WhenSuccess()
        {
            //Arrange
            _cityRepositoryMock.Setup(
                x => x.GetByIdAsync(It.IsAny<int>())
            ).ReturnsAsync(
                _fixture.Build<City>()
                    .With(x => x.Id,_command.id)
                    .Without(x => x.Hotels)
                .Create());
            _cityRepositoryMock.Setup(
                x => x.UpdateAsync(It.IsAny<City>())
            );
            //Act
            CityDto result = await _handler.Handle(_command, default);
            //Assert
            _cityRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<City>()), Times.Once);
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
            _cityRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<City>()), Times.Never);
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
                x.UpdateAsync(
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