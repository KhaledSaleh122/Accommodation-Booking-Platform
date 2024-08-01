using Application.CommandsAndQueries.BookingCQ.Commands.Confirm;
using Application.Execptions;
using AutoFixture;
using Domain.Abstractions;
using Domain.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using Stripe;
using System.Net.Mail;

namespace ABP.Application.Tests.BookingTests.Commands
{
    public class ConfirmBookingHandlerTests
    {
        private readonly Mock<IBookingRepository> _bookingRepositoryMock;
        private readonly Mock<IHotelRepository> _hotelRepositoryMock;
        private readonly Mock<UserManager<User>> _userManagerMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly ConfirmBookingHandler _handler;
        private readonly Fixture _fixture;

        public ConfirmBookingHandlerTests()
        {
            _bookingRepositoryMock = new();
            _hotelRepositoryMock = new();
            _emailServiceMock = new();
            _emailServiceMock.Setup(x => x.SendEmailAsync(It.IsAny<MailMessage>())).Returns(Task.CompletedTask);
            var store = new Mock<IUserStore<User>>();
            _userManagerMock = new(store.Object, null, null, null, null, null, null, null, null);
            _configurationMock = new();
            _fixture = new Fixture();
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            _fixture.Customize<User>(c => c
                .With(x => x.Email, _fixture.Create<MailAddress>().Address)
            );
            _fixture.Customize<Booking>(c => c
                .With(x => x.StartDate, DateOnly.FromDateTime(DateTime.Today))
                .With(x => x.EndDate, DateOnly.FromDateTime(DateTime.Today.AddDays(1)))
                .With(x => x.PaymentIntentId, "pi_123")
            );
            _fixture.Customize<SpecialOffer>(c => c
                .With(x => x.ExpireDate, DateOnly.FromDateTime(DateTime.Today.AddDays(1)))
            );
            _configurationMock.Setup(x => x.GetSection("Mail:Email").Value)
                .Returns("test@example.com");

            _configurationMock.Setup(x => x.GetSection("Mail:AppPassword").Value)
                .Returns("password");

            _configurationMock.Setup(x => x.GetSection("Mail:Host").Value)
                .Returns("smtp.example.com");

            _configurationMock.Setup(x => x.GetSection("Mail:Port").Value)
                .Returns("587");
            _handler = new ConfirmBookingHandler(
                _bookingRepositoryMock.Object,
                _configurationMock.Object,
                _userManagerMock.Object,
                _hotelRepositoryMock.Object,
                _emailServiceMock.Object);
        }

        [Fact]
        public async Task Handle_Should_SendConfirmationEmail_When_PaymentIntentSucceeded()
        {
            // Arrange
            var command = _fixture.Build<ConfirmBookingCommand>()
            .With(x => x.Event, new Stripe.Event
            {
                Type = Events.PaymentIntentSucceeded,
                Data = new Stripe.EventData
                {
                    Object = new PaymentIntent { Id = _fixture.Create<Guid>().ToString() }
                }
            })
            .Create();
            var booking = _fixture.Create<Booking>();
            var user = _fixture.Create<User>();
            var hotel = _fixture.Create<Hotel>();

            _bookingRepositoryMock.Setup(x => x.GetByPaymentIntentIdAsync(It.IsAny<string>()))
                .ReturnsAsync(booking);

            _userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(user);

            _hotelRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((hotel, 4.5d));
            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            _bookingRepositoryMock.Verify(x => x.GetByPaymentIntentIdAsync(It.IsAny<string>()), Times.Once);
            _userManagerMock.Verify(x => x.FindByIdAsync(It.IsAny<string>()), Times.Once);
            _hotelRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
        }

        [Fact]
        public async Task Handle_Should_ThrowErrorException_When_PaymentIntentNotFound()
        {
            // Arrange
            var command = _fixture.Build<ConfirmBookingCommand>()
                .With(x => x.Event, new Stripe.Event
                {
                    Type = Events.PaymentIntentSucceeded,
                    Data = new Stripe.EventData
                    {
                        Object = null
                    }
                })
                .Create();
            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<ErrorException>()
                .WithMessage("Payment Intent not found");
        }

        [Fact]
        public async Task Handle_Should_ThrowErrorException_When_BookingNotFound()
        {
            // Arrange
            var command = _fixture.Build<ConfirmBookingCommand>()
                .With(x => x.Event, new Stripe.Event
                {
                    Type = Events.PaymentIntentSucceeded,
                    Data = new Stripe.EventData
                    {
                        Object = new PaymentIntent { Id = "pi_123" }
                    }
                })
                .Create();

            _bookingRepositoryMock.Setup(x => x.GetByPaymentIntentIdAsync(It.IsAny<string>()))
                .ReturnsAsync((Booking?)null);

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<ErrorException>()
                .WithMessage("Booking with Payment Intent ID 'pi_123' not found");
        }

        [Fact]
        public async Task Handle_Should_ThrowErrorException_When_UserNotFound()
        {
            // Arrange
            var command = _fixture.Build<ConfirmBookingCommand>()
                .With(x => x.Event, new Stripe.Event
                {
                    Type = Events.PaymentIntentSucceeded,
                    Data = new Stripe.EventData
                    {
                        Object = new PaymentIntent { Id = "pi_123" }
                    }
                })
                .Create();

            var booking = _fixture.Create<Booking>();

            _bookingRepositoryMock.Setup(x => x.GetByPaymentIntentIdAsync(It.IsAny<string>()))
                .ReturnsAsync(booking);

            _userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync((User?)null);

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<ErrorException>()
                .WithMessage($"User with Booking ID '{booking.Id}' not found");
        }

        [Fact]
        public async Task Handle_Should_ThrowErrorException_When_HotelNotFound()
        {
            // Arrange
            var command = _fixture.Build<ConfirmBookingCommand>()
                .With(x => x.Event, new Stripe.Event
                {
                    Type = Events.PaymentIntentSucceeded,
                    Data = new Stripe.EventData
                    {
                        Object = new PaymentIntent { Id = "pi_123" }
                    }
                })
                .Create();

            var booking = _fixture.Create<Booking>();

            var user = _fixture.Create<User>();

            _bookingRepositoryMock.Setup(x => x.GetByPaymentIntentIdAsync(It.IsAny<string>()))
                .ReturnsAsync(booking);

            _userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(user);

            _hotelRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(((Hotel, double)?)null);

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<ErrorException>()
                .WithMessage($"Hotel with ID '{booking.BookingRooms.First().HotelId}' not found");
        }
    }
}
