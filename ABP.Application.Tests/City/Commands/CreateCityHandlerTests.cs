using Application.CommandsAndQueries.CityCQ.Commands.Create;
using Application.Dtos.CityDtos;
using Application.Execptions;
using AutoFixture;
using Domain.Abstractions;
using Domain.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Text;

namespace ABP.Application.Tests
{
    public class CreateCityHandlerTests
    {
        private readonly Mock<ICityRepository> _cityRepositoryMock;
        private readonly Mock<IImageService> _imageRepositoryMock;
        private readonly Mock<ITransactionService> _transactionServiceMock;
        private readonly CreateCityHandler _handler;
        private readonly IFixture _fixture;
        private readonly CreateCityCommand _command;
        public CreateCityHandlerTests()
        {
            _cityRepositoryMock = new();
            _imageRepositoryMock = new();
            _transactionServiceMock = new();
            _fixture = new Fixture();
            var bytes = Encoding.UTF8.GetBytes("This is a dummy file");
            IFormFile testFile = new FormFile(new MemoryStream(bytes), 0, bytes.Length, "Thumbnail", "Thumbnail.png");
            _command = new CreateCityCommand()
            {
                PostOffice = _fixture.Create<string>(),
                Name = _fixture.Create<string>(),
                Country = _fixture.Create<string>(),
                Thumbnail = testFile
            };
            _handler = new CreateCityHandler(
                _cityRepositoryMock.Object, _imageRepositoryMock.Object, _transactionServiceMock.Object);
        }
        [Fact]
        public async Task Handler_Should_ReturnCreatedCity_WhenSuccess()
        {
            //Act
            CityDto result = await _handler.Handle(_command, default);
            //Assert
            _cityRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<City>()), Times.Once);
            result.Should().NotBeNull();
        }
        [Fact]
        public async Task Handler_Should_ThrowsException_WhenFail()
        {
            //Arrange
            _cityRepositoryMock.Setup(x =>
                x.CreateAsync(
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