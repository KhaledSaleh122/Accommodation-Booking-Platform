using Application.CommandsAndQueries.HotelCQ.Commands.Create;
using Application.Dtos.HotelDtos;
using Application.Execptions;
using AutoFixture;
using Domain.Abstractions;
using Domain.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Text;

namespace ABP.Application.Tests.HotelTests.commands
{
    public class CreateHotelHandlerTests
    {
        private readonly Mock<IHotelRepository> _hotelRepositoryMock;
        private readonly Mock<ICityRepository> _cityRepositoryMock;
        private readonly Mock<IImageService> _imageRepositoryMock;
        private readonly Mock<ITransactionService> _transactionServiceMock;
        private readonly CreateHotelHandler _handler;
        private readonly IFixture _fixture;
        private readonly CreateHotelCommand _command;
        public CreateHotelHandlerTests()
        {
            _hotelRepositoryMock = new();
            _imageRepositoryMock = new();
            _transactionServiceMock = new();
            _cityRepositoryMock = new();
            _fixture = new Fixture();
            var bytes = Encoding.UTF8.GetBytes("This is a dummy file");
            IFormFile testFile = new FormFile(new MemoryStream(bytes), 0, bytes.Length, "dummy", "dummy.png");
            _command = new CreateHotelCommand()
            {
                Name = _fixture.Create<string>(),
                Address = _fixture.Create<string>(),
                Description = _fixture.Create<string>(),
                CityId = _fixture.Create<int>(),
                HotelType = _fixture.Create<int>(),
                Images = [testFile],
                Owner = _fixture.Create<string>(),
                PricePerNight = _fixture.Create<int>(),
                Thumbnail = testFile
            };
            _handler = new CreateHotelHandler(
                _hotelRepositoryMock.Object, _imageRepositoryMock.Object, _cityRepositoryMock.Object, _transactionServiceMock.Object);
        }
        [Fact]
        public async Task Handler_Should_ReturnCreatedHotel_WhenSuccess()
        {
            //Act
            HotelMinDto result = await _handler.Handle(_command, default);
            //Assert
            _hotelRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Hotel>()), Times.Once);
            result.Should().NotBeNull();
        }
        [Fact]
        public async Task Handler_Should_ThrowsException_WhenFail()
        {
            //Arrange
            _hotelRepositoryMock.Setup(x =>
                x.CreateAsync(
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