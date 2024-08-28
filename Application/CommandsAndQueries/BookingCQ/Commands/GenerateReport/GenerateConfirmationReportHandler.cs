using Application.Exceptions;
using Application.Execptions;
using Domain.Abstractions;
using Domain.Entities;
using iTextSharp.text;
using iTextSharp.text.pdf;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Stripe;

namespace Application.CommandsAndQueries.BookingCQ.Commands.GenerateReport
{
    public class GenerateConfirmationReportHandler : IRequestHandler<GenerateConfirmationReportQuery, byte[]>
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IHotelRepository _hotelRepository;
        private readonly UserManager<User> _userManager;
        private readonly IPaymentService<PaymentIntent, PaymentIntentCreateOptions> _paymentService;
        private readonly IInvoiceGeneraterService _invoiceGeneraterService;

        public GenerateConfirmationReportHandler(IBookingRepository bookingRepository,
            IPaymentService<PaymentIntent, PaymentIntentCreateOptions> paymentService,
            UserManager<User> userManager,
            IHotelRepository hotelRepository,
            IInvoiceGeneraterService invoiceGeneraterService)
        {
            _bookingRepository = bookingRepository ?? throw new ArgumentNullException(nameof(bookingRepository));
            _paymentService = paymentService ?? throw new ArgumentNullException(nameof(paymentService));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _hotelRepository = hotelRepository ?? throw new ArgumentNullException(nameof(hotelRepository));
            _invoiceGeneraterService = invoiceGeneraterService ?? throw new ArgumentNullException(nameof(invoiceGeneraterService));
        }

        public async Task<byte[]> Handle(GenerateConfirmationReportQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var booking = await _bookingRepository.GetByIdAsync(request.UserId, request.BookingId)
                    ?? throw new NotFoundException("Booking not found!");

                var paymentIntent = await _paymentService.GetAsync(booking.PaymentIntentId, cancellationToken)
                    ?? throw new ErrorException("We encountered an issue while processing your payment. Please contact customer support.")
                    {
                        StatusCode = 500
                    };

                if (paymentIntent.Status != "succeeded")
                    throw new ErrorException("Payment has not been made yet")
                    {
                        StatusCode = 409
                    };

                var user = await _userManager.FindByIdAsync(request.UserId) ?? throw new NotFoundException("User not found!");
                var (hotel, avgRate) = await _hotelRepository.GetByIdAsync(booking.BookingRooms.First().HotelId)
                    ?? throw new ErrorException("Hotel not found") { StatusCode = 500 };


                return _invoiceGeneraterService.GenerateInvoicePdf(booking,user,hotel);
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (ErrorException)
            {
                throw;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
