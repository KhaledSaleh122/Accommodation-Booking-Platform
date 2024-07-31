using Application.CommandsAndQueries.SpecialOfferCQ.Commands.Create;
using Application.Dtos.SpecialOfferDtos;
using Application.Exceptions;
using Application.Execptions;
using AutoFixture;
using Domain.Abstractions;
using Domain.Entities;
using Domain.Enums;
using FluentAssertions;
using Moq;

namespace ABP.Application.Tests.SpecialOfferTests.Commands
{
    public class CreateSpecialOfferHandlerTests
    {
        private readonly Mock<ISpecialOfferRepository> _specialOfferRepositoryMock;
        private readonly Mock<IHotelRepository> _hotelRepositoryMock;
        private readonly CreateSpecialOfferHandler _handler;
        private readonly IFixture _fixture;
        private readonly CreateSpecialOfferCommand _command;
        public CreateSpecialOfferHandlerTests()
        {
            _specialOfferRepositoryMock = new();
            _hotelRepositoryMock = new();
            _fixture = new Fixture();
            _command = new CreateSpecialOfferCommand()
            {
                DiscountPercentage = _fixture.Create<int>(),
                ExpireDate = DateOnly.FromDateTime(_fixture.Create<DateTime>()),
                Id = _fixture.Create<string>(),
                OfferType = _fixture.Create<OfferType>(),
                hotelId = _fixture.Create<int>(),
            };
            _handler = new CreateSpecialOfferHandler(
                _specialOfferRepositoryMock.Object, _hotelRepositoryMock.Object);
        }
        [Fact]
        public async Task Handler_Should_ReturnCreatedSpecialOffer_WhenSuccess()
        {
            //Act
            _hotelRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((new Hotel(), default));
            _specialOfferRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<string>())).ReturnsAsync((SpecialOffer?)null);
            _specialOfferRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<SpecialOffer>())).ReturnsAsync(new SpecialOffer());
            SpecialOfferDto result = await _handler.Handle(_command, default);
            //Assert
            _specialOfferRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<SpecialOffer>()), Times.Once);
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task Handler_Should_ThrowsErrorException_WhenSpecialOfferExistsWithTheId()
        {
            //Arrange
            _hotelRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((new Hotel(), default));
            _specialOfferRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(new SpecialOffer());
            //Act
            Func<Task> result = async () => await _handler.Handle(_command, default);
            //Assert
            _specialOfferRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<SpecialOffer>()), Times.Never);
            await result.Should().ThrowAsync<ErrorException>();
        }

        [Fact]
        public async Task Handler_Should_ThrowsNotFoundException_WhenHotelIsNull()
        {
            //Arrange
            _hotelRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(((Hotel, double)?)null);
            _specialOfferRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<string>())).ReturnsAsync((SpecialOffer?)null);
            //Act
            Func<Task> result = async () => await _handler.Handle(_command, default);
            //Assert
            _specialOfferRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<SpecialOffer>()), Times.Never);
            await result.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task Handler_Should_ThrowsException_WhenFail()
        {
            //Arrange
            _hotelRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((new Hotel(), default));
            _specialOfferRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<string>())).ReturnsAsync((SpecialOffer?)null);
            _specialOfferRepositoryMock.Setup(x =>
                x.CreateAsync(
                    It.IsAny<SpecialOffer>()
                )
            ).Throws<Exception>();
            //Act
            Func<Task> result = async () => await _handler.Handle(_command, default);
            //Assert
            await result.Should().ThrowAsync<ErrorException>();
        }
    }
}