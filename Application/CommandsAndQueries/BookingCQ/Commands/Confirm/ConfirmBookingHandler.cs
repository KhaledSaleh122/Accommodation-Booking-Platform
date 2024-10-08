﻿using Application.Exceptions;
using Application.Execptions;
using Domain.Abstractions;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Stripe;
using System.Net.Mail;

namespace Application.CommandsAndQueries.BookingCQ.Commands.Confirm
{
    public class ConfirmBookingHandler : IRequestHandler<ConfirmBookingCommand>
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IUserManager _userManager;
        private readonly IConfiguration _configuration;
        private readonly IHotelRepository _hotelRepository;
        private readonly IEmailService _emailService;

        public ConfirmBookingHandler(
            IBookingRepository bookingRepository,
            IConfiguration configuration,
            IUserManager userManager,
            IHotelRepository hotelRepository,
            IEmailService emailService)
        {
            _bookingRepository = bookingRepository ?? throw new ArgumentNullException(nameof(bookingRepository));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _hotelRepository = hotelRepository ?? throw new ArgumentNullException(nameof(hotelRepository));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        }

        public async Task Handle(ConfirmBookingCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if (request.Event.Type == Events.PaymentIntentSucceeded)
                {
                    var paymentIntent = request.Event.Data.Object as PaymentIntent ??
                        throw new ErrorException("Payment Intent not found")
                        {
                            StatusCode = StatusCodes.Status500InternalServerError,
                            LoggerLevel = LoggerLevel.Critical
                        };
                    var booking = await _bookingRepository.GetByPaymentIntentIdAsync(paymentIntent.Id) ??
                        throw new ErrorException($"Booking with Payment Intent ID '{paymentIntent.Id}' not found")
                        {
                            StatusCode = StatusCodes.Status500InternalServerError,
                            LoggerLevel = LoggerLevel.Critical
                        };
                    var user = await _userManager.FindByIdAsync(booking.UserId) ??
                        throw new ErrorException($"User with Booking ID '{booking.Id}' not found")
                        {
                            StatusCode = StatusCodes.Status500InternalServerError,
                            LoggerLevel = LoggerLevel.Critical
                        };
                    var (hotel, avgRating) = await _hotelRepository.GetByIdAsync(booking.BookingRooms.First().HotelId) ??
                        throw new ErrorException($"Hotel with ID '{booking.BookingRooms.First().HotelId}' not found")
                        {
                            StatusCode = StatusCodes.Status500InternalServerError,
                            LoggerLevel = LoggerLevel.Critical
                        };
                    await SendBookingConfirmationEmail(booking, user, hotel);
                }
            }
            catch (ErrorException)
            {
                throw;
            }
            catch (Exception exception)
            {
                throw new ErrorException("Error happend during confirm the booking", exception);
            }
        }
        private async Task SendBookingConfirmationEmail(Booking booking, User user, Hotel hotel)
        {
            string fromAddress = _configuration.GetValue<string>("Mail:Email")!;
            string toAddress = user.Email!;
            string subject = "Booking Confirmation";
            string specialOffer = booking.SpecialOfferId is not null ? $"Special Offer: {booking.SpecialOfferId}" : "";
            string body = $"""
                    Dear {user.UserName},

                    We are pleased to inform you that your booking has been confirmed. Here are the details of your booking:
                    Hotel: {hotel.Name}
                    Country: {hotel.City.Country}
                    City: {hotel.City.Name}
                    Address: {hotel.Address}
                    Rooms Numbers: {String.Join(",", booking.BookingRooms.Select(x => x.RoomNumber))}
                    Start Date: {booking.StartDate}
                    End Date: {booking.EndDate}
                    {specialOffer}
                    Total Price: {booking.OriginalTotalPrice}
                    Discounted Total Price: {booking.DiscountedTotalPrice}

                    We have also received your payment successfully.

                    Thank you for choosing our service. We look forward to hosting you.

                    Best regards,
                    Accommodation Booking Platform Team
                    """;

            var mail = new MailMessage
            {
                From = new MailAddress(fromAddress),
                Subject = subject,
                Body = body,
            };
            mail.To.Add(toAddress);
            // Send the email
            await _emailService.SendEmailAsync(mail);
        }
    }
}
