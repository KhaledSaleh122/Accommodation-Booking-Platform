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

        public GenerateConfirmationReportHandler(IBookingRepository bookingRepository,
            IPaymentService<PaymentIntent, PaymentIntentCreateOptions> paymentService,
            UserManager<User> userManager,
            IHotelRepository hotelRepository)
        {
            _bookingRepository = bookingRepository ?? throw new ArgumentNullException(nameof(bookingRepository));
            _paymentService = paymentService ?? throw new ArgumentNullException(nameof(paymentService));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _hotelRepository = hotelRepository ?? throw new ArgumentNullException(nameof(hotelRepository));
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

                using var memoryStream = new MemoryStream();
                Document document = new Document(PageSize.A4, 50, 50, 25, 25);
                PdfWriter.GetInstance(document, memoryStream).CloseStream = false;
                document.Open();

                // Add header section with company info and invoice header
                AddHeaderSection(document, booking.Id);

                // Add billing and shipping information
                AddCustomerAndHotelInfo(document, user, hotel);

                // Add table for items
                AddAccommodationAndSpecialOfferInfo(document, booking, string.Join(", ", booking.BookingRooms.Select(x => x.RoomNumber).ToList()), booking.SpecialOffer);

                // Add totals
                AddTotals(document, booking);

                // Add footer
                AddFooter(document);

                document.Close();

                byte[] pdfBytes = memoryStream.ToArray();

                return pdfBytes;
            }
            catch (NotFoundException) {
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

        private void AddHeaderSection(Document document, int invoiceNumber)
        {
            PdfPTable headerTable = new PdfPTable(2)
            {
                WidthPercentage = 100
            };
            headerTable.SetWidths(new float[] { 1, 1 });

            PdfPCell companyInfoCell = new PdfPCell(new Phrase("Accommodation Booking Platform\nPalestine\nTulkarm, Tulkarm\n0569361712\nKhaled.S.Saleh@hotmail.com",
                new Font(Font.FontFamily.HELVETICA, 12, Font.NORMAL, BaseColor.BLACK)))
            {
                Border = Rectangle.NO_BORDER,
                HorizontalAlignment = Element.ALIGN_LEFT
            };
            headerTable.AddCell(companyInfoCell);

            PdfPTable invoiceTable = new PdfPTable(1);

            PdfPCell invoiceHeaderCell = new PdfPCell(new Phrase("INVOICE",
                new Font(Font.FontFamily.HELVETICA, 24, Font.BOLD, new BaseColor(46, 204, 113))))
            {
                Border = Rectangle.NO_BORDER,
                HorizontalAlignment = Element.ALIGN_RIGHT
            };
            invoiceTable.AddCell(invoiceHeaderCell);

            PdfPCell invoiceDetailsCell = new PdfPCell(new Phrase($"Invoice No. {invoiceNumber}\nDate: {DateOnly.FromDateTime(DateTime.Now)}",
                new Font(Font.FontFamily.HELVETICA, 12, Font.NORMAL, BaseColor.BLACK)))
            {
                Border = Rectangle.NO_BORDER,
                HorizontalAlignment = Element.ALIGN_RIGHT
            };
            invoiceTable.AddCell(invoiceDetailsCell);

            PdfPCell invoiceCell = new PdfPCell(invoiceTable)
            {
                Border = Rectangle.NO_BORDER
            };
            headerTable.AddCell(invoiceCell);

            document.Add(headerTable);
            document.Add(new Paragraph("\n")); // Spacer
        }

        private void AddCustomerAndHotelInfo(Document document, User user, Hotel hotel)
        {
            PdfPTable table = new PdfPTable(2)
            {
                WidthPercentage = 100
            };
            table.SetWidths(new float[] { 1, 1 });

            PdfPCell customerInfoCell = new PdfPCell(new Phrase("Customer Info:", new Font(Font.FontFamily.HELVETICA, 12, Font.BOLD, BaseColor.WHITE)))
            {
                BackgroundColor = new BaseColor(46, 204, 113)
            };
            table.AddCell(customerInfoCell);

            PdfPCell hotelInfoCell = new PdfPCell(new Phrase("Hotel Info:", new Font(Font.FontFamily.HELVETICA, 12, Font.BOLD, BaseColor.WHITE)))
            {
                BackgroundColor = new BaseColor(46, 204, 113)
            };
            table.AddCell(hotelInfoCell);

            table.AddCell(new Phrase($"{user.UserName}\n{user.Email}\n", new Font(Font.FontFamily.HELVETICA, 12, Font.NORMAL, BaseColor.BLACK)));
            table.AddCell(new Phrase($"{hotel.Name}\n{hotel.City.Country}\n{hotel.City.Name}\n{hotel.Address}, {hotel.City.PostOffice}", new Font(Font.FontFamily.HELVETICA, 12, Font.NORMAL, BaseColor.BLACK)));

            document.Add(table);
            document.Add(new Paragraph("\n")); // Spacer
        }

        private void AddAccommodationAndSpecialOfferInfo(Document document, Booking booking, string rooms, SpecialOffer? specialOffer)
        {
            PdfPTable table = new PdfPTable(2)
            {
                WidthPercentage = 100
            };
            table.SetWidths(new float[] { 1, 1 });

            PdfPCell accommodationCell = new PdfPCell(new Phrase("Accommodation Info:", new Font(Font.FontFamily.HELVETICA, 12, Font.BOLD, BaseColor.WHITE)))
            {
                BackgroundColor = new BaseColor(46, 204, 113)
            };
            table.AddCell(accommodationCell);

            if (specialOffer != null)
            {
                PdfPCell specialOfferCell = new PdfPCell(new Phrase("Special Offer Info:", new Font(Font.FontFamily.HELVETICA, 12, Font.BOLD, BaseColor.WHITE)))
                {
                    BackgroundColor = new BaseColor(46, 204, 113)
                };
                table.AddCell(specialOfferCell);
            }
            else
            {
                PdfPCell emptyCell = new PdfPCell(new Phrase("", new Font(Font.FontFamily.HELVETICA, 12, Font.BOLD, BaseColor.WHITE)))
                {
                    BackgroundColor = new BaseColor(46, 204, 113)
                };
                table.AddCell(emptyCell);
            }

            table.AddCell(new Phrase($"{rooms}\n{booking.StartDate:dd/MM/yyyy}\n{booking.EndDate:dd/MM/yyyy}", new Font(Font.FontFamily.HELVETICA, 12, Font.NORMAL, BaseColor.BLACK)));

            if (specialOffer != null)
            {
                table.AddCell(new Phrase($"{specialOffer.Id}\n{specialOffer.OfferType}\n{specialOffer.DiscountPercentage}%", new Font(Font.FontFamily.HELVETICA, 12, Font.NORMAL, BaseColor.BLACK)));
            }
            else
            {
                table.AddCell(new Phrase("", new Font(Font.FontFamily.HELVETICA, 12, Font.NORMAL, BaseColor.BLACK)));
            }

            document.Add(table);
            document.Add(new Paragraph("\n")); // Spacer
        }

        private void AddTotals(Document document, Booking booking)
        {
            PdfPTable totalsTable = new PdfPTable(2)
            {
                WidthPercentage = 100
            };
            totalsTable.SetWidths(new float[] { 1, 1 });

            PdfPCell emptyCell = new PdfPCell(new Phrase(""))
            {
                Border = Rectangle.NO_BORDER
            };
            totalsTable.AddCell(emptyCell);

            PdfPCell totalsHeaderCell = new PdfPCell(new Phrase("Totals",
                new Font(Font.FontFamily.HELVETICA, 12, Font.BOLD, BaseColor.WHITE)))
            {
                Border = Rectangle.NO_BORDER,
                BackgroundColor = new BaseColor(46, 204, 113),
                HorizontalAlignment = Element.ALIGN_RIGHT
            };
            totalsTable.AddCell(totalsHeaderCell);

            totalsTable.AddCell(emptyCell);
            totalsTable.AddCell(new PdfPCell(new Phrase($"DISCOUNT: {booking.OriginalTotalPrice - booking.DiscountedTotalPrice:C2}",
                new Font(Font.FontFamily.HELVETICA, 12, Font.NORMAL, BaseColor.BLACK)))
            {
                HorizontalAlignment = Element.ALIGN_RIGHT,
                Border = Rectangle.NO_BORDER
            });

            totalsTable.AddCell(emptyCell);
            totalsTable.AddCell(new PdfPCell(new Phrase($"TOTAL WITH DISCOUNT: {booking.DiscountedTotalPrice:C2}",
                new Font(Font.FontFamily.HELVETICA, 12, Font.NORMAL, BaseColor.BLACK)))
            {
                HorizontalAlignment = Element.ALIGN_RIGHT,
                Border = Rectangle.NO_BORDER
            });

            document.Add(totalsTable);
        }

        private void AddFooter(Document document)
        {
            Paragraph footer = new Paragraph("Thank you for your business!",
                new Font(Font.FontFamily.HELVETICA, 12, Font.BOLD, BaseColor.BLACK))
            {
                Alignment = Element.ALIGN_CENTER
            };
            document.Add(footer);
        }
    }
}
