using Application.CommandsAndQueries.BookingCQ.Commands.Create;
using Application.Exceptions;
using Application.Execptions;
using AutoFixture;
using Domain.Abstractions;
using Domain.Entities;
using FluentAssertions;
using Moq;
using Stripe;

namespace ABP.Application.Tests.BookingTests.Commands
{
    public class CreateRoomBookingHandlerTests
    {
        private readonly Mock<IBookingRepository> _bookingRepositoryMock;
        private readonly Mock<IHotelRepository> _hotelRepositoryMock;
        private readonly Mock<IHotelRoomRepository> _hotelRoomRepositoryMock;
        private readonly Mock<ISpecialOfferRepository> _specialOfferRepositoryMock;
        private readonly Mock<IPaymentService<PaymentIntent, PaymentIntentCreateOptions>> _paymentServiceMock;
        private readonly CreateRoomBookingCommand _command;
        private readonly CreateRoomBookingHandler _handler;
        private readonly Mock<ITransactionService> _transactionServiceMock;
        private readonly Fixture _fixture;

        public CreateRoomBookingHandlerTests()
        {
            _transactionServiceMock = new();
            _bookingRepositoryMock = new();
            _hotelRepositoryMock = new();
            _hotelRoomRepositoryMock = new();
            _specialOfferRepositoryMock = new();
            _paymentServiceMock = new();
            _fixture = new Fixture();
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            _fixture.Customize<CreateRoomBookingCommand>(c => c
                .With(x => x.StartDate, DateOnly.FromDateTime(DateTime.Today))
                .With(x => x.EndDate, DateOnly.FromDateTime(DateTime.Today.AddDays(1)))
            );
            _command = _fixture.Create<CreateRoomBookingCommand>();
            _fixture.Customize<SpecialOffer>(c => c
                .With(x => x.ExpireDate, DateOnly.FromDateTime(DateTime.Today.AddDays(1)))
                .With(x => x.HotelId, _command.HotelId)
            );
            _fixture.Customize<Booking>(c => c
                .With(x => x.StartDate, DateOnly.FromDateTime(DateTime.Today))
                .With(x => x.EndDate, DateOnly.FromDateTime(DateTime.Today.AddDays(1)))
            );
            _fixture.Customize<Hotel>(c => c
                .With(x => x.Rooms, _command.RoomsNumbers.Select(roomNumber => new Room { RoomNumber = roomNumber }).ToList())
            );
            _handler = new CreateRoomBookingHandler(
                _bookingRepositoryMock.Object,
                _hotelRoomRepositoryMock.Object,
                _transactionServiceMock.Object,
                _hotelRepositoryMock.Object,
                _specialOfferRepositoryMock.Object,
                _paymentServiceMock.Object);
        }

        [Fact]
        public async Task Handler_Should_ReturnBookingRequestDto_WhenSuccess()
        {
            // Arrange
            var hotel = _fixture.Create<Hotel>();

            _hotelRepositoryMock.Setup(x => x.FindRoomsAsync(It.IsAny<int>(), It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(hotel);

            _hotelRoomRepositoryMock.Setup(x => x.IsRoomAvailableAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>()))
                .ReturnsAsync(true);
            _specialOfferRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(_fixture.Create<SpecialOffer>());

            var paymentIntent = new PaymentIntent { Id = _fixture.Create<Guid>().ToString(), ClientSecret = _fixture.Create<Guid>().ToString() };
            _paymentServiceMock.Setup(x => x.CreateAsync(It.IsAny<PaymentIntentCreateOptions>(), default))
                .ReturnsAsync(paymentIntent);

            _bookingRepositoryMock.Setup(x => x.CreateRoomBookingAsync(It.IsAny<Booking>()))
                .ReturnsAsync(_fixture.Create<Booking>());

            // Act
            var result = await _handler.Handle(_command, default);

            // Assert
            result.Should().NotBeNull();
            result.ClientSecret.Should().Be(paymentIntent.ClientSecret);
            result.PaymentIntentId.Should().Be(paymentIntent.Id);
        }

        [Fact]
        public async Task Handler_Should_ThrowNotFoundException_WhenHotelNotFound()
        {
            // Arrange
            _hotelRepositoryMock.Setup(x => x.FindRoomsAsync(It.IsAny<int>(), It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync((Hotel?)null);

            // Act
            Func<Task> act = async () => await _handler.Handle(_command, default);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>().WithMessage("Hotel not found!");
        }

        [Fact]
        public async Task Handler_Should_ThrowNotFoundException_WhenRoomNotFound()
        {
            // Arrange
            var hotel = _fixture.Create<Hotel>();
            hotel.Rooms.First().RoomNumber = _fixture.Create<Guid>().ToString();
            _hotelRepositoryMock.Setup(x => x.FindRoomsAsync(It.IsAny<int>(), It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(hotel);

            // Act
            Func<Task> act = async () => await _handler.Handle(_command, default);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>().WithMessage($"Room with number '{_command.RoomsNumbers.First()}' not found!");
        }

        [Fact]
        public async Task Handler_Should_ThrowErrorException_WhenRoomNotAvailable()
        {
            // Arrange
            var hotel = _fixture.Create<Hotel>();

            _hotelRepositoryMock.Setup(x => x.FindRoomsAsync(It.IsAny<int>(), It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(hotel);

            _hotelRoomRepositoryMock.Setup(x => x.IsRoomAvailableAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>()))
                .ReturnsAsync(false);

            // Act
            Func<Task> act = async () => await _handler.Handle(_command, default);

            // Assert
            await act.Should().ThrowAsync<ErrorException>().WithMessage($"Room with number {_command.RoomsNumbers.First()} not available for selected date");
        }

        [Fact]
        public async Task Handler_Should_ThrowErrorException_WhenOfferNotValidForHotel()
        {
            // Arrange
            var hotel = _fixture.Create<Hotel>();

            _hotelRepositoryMock.Setup(x => x.FindRoomsAsync(It.IsAny<int>(), It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(hotel);

            _hotelRoomRepositoryMock.Setup(x => x.IsRoomAvailableAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>()))
                .ReturnsAsync(true);

            var specialOffer = _fixture.Create<SpecialOffer>();
            specialOffer.HotelId = hotel.Id + 1;
            _specialOfferRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(specialOffer);

            // Act
            Func<Task> act = async () => await _handler.Handle(_command, default);

            // Assert
            await act.Should().ThrowAsync<ErrorException>().WithMessage("The offer code is not valid for this hotel.");
        }

        [Fact]
        public async Task Handler_Should_ThrowErrorException_WhenOfferExpired()
        {
            // Arrange
            var hotel = _fixture.Create<Hotel>();

            _hotelRepositoryMock.Setup(x => x.FindRoomsAsync(It.IsAny<int>(), It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(hotel);

            _hotelRoomRepositoryMock.Setup(x => x.IsRoomAvailableAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>()))
                .ReturnsAsync(true);

            var specialOffer = _fixture.Create<SpecialOffer>();
            specialOffer.ExpireDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1));
            _specialOfferRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(specialOffer);

            // Act
            Func<Task> act = async () => await _handler.Handle(_command, default);

            // Assert
            await act.Should().ThrowAsync<ErrorException>().WithMessage("The offer is expired");
        }

        [Fact]
        public async Task Handler_Should_ThrowErrorException_WhenGeneralErrorOccurs()
        {
            // Arrange
            var hotel = _fixture.Create<Hotel>();
            hotel.Rooms.Add(new Room { RoomNumber = _command.RoomsNumbers.First() });

            _hotelRepositoryMock.Setup(x => x.FindRoomsAsync(It.IsAny<int>(), It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(hotel);

            _hotelRoomRepositoryMock.Setup(x => x.IsRoomAvailableAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>()))
                .ReturnsAsync(true);

            _specialOfferRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(_fixture.Create<SpecialOffer>());

            _paymentServiceMock.Setup(x => x.CreateAsync(It.IsAny<PaymentIntentCreateOptions>(), default))
                .ThrowsAsync(new Exception());

            // Act
            Func<Task> act = async () => await _handler.Handle(_command, default);

            // Assert
            await act.Should().ThrowAsync<ErrorException>().WithMessage("Error during creating the room booking");
        }
    }
}
