using Application.Dtos.BookingDtos;
using Application.Exceptions;
using Application.Execptions;
using AutoMapper;
using Domain.Abstractions;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Stripe;

namespace Application.CommandsAndQueries.BookingCQ.Commands.Create
{
    internal class CreateRoomBookingHandler : IRequestHandler<CreateRoomBookingCommand, BookingRequestDto>
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IHotelRepository _hotelRepository;
        private readonly IHotelRoomRepository _hotelRoomRepository;
        private readonly ISpecialOfferRepository _specialOfferRepository;
        private readonly IPaymentService<PaymentIntent, PaymentIntentCreateOptions> _paymentService;
        private readonly IUserManager _userManager;
        private readonly IMapper _mapper;

        public CreateRoomBookingHandler(
            IBookingRepository bookingRepository,
            IHotelRoomRepository hotelRoomRepository,
            ITransactionService transactionService,
            IHotelRepository hotelRepository,
            ISpecialOfferRepository specialOfferRepository,
            IPaymentService<PaymentIntent, PaymentIntentCreateOptions> paymentService,
            IUserManager userManager)
        {
            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Booking, BookingDto>()
                .ForMember(dest => dest.HotelId,
                opt => opt.MapFrom(src => src.BookingRooms.First().HotelId))
                .ForMember(dest => dest.Rooms,
                opt => opt.MapFrom(src => src.BookingRooms.Select(x => x.RoomNumber).ToList()));
                cfg.CreateMap<CreateRoomBookingCommand, Booking>();
            });
            _mapper = configuration.CreateMapper();
            _bookingRepository = bookingRepository ?? throw new ArgumentNullException(nameof(bookingRepository));
            _hotelRoomRepository = hotelRoomRepository ?? throw new ArgumentNullException(nameof(hotelRoomRepository));
            _hotelRepository = hotelRepository ?? throw new ArgumentNullException(nameof(hotelRepository));
            _specialOfferRepository = specialOfferRepository ?? throw new ArgumentNullException(nameof(specialOfferRepository));
            _paymentService = paymentService ?? throw new ArgumentNullException(nameof(paymentService));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        public async Task<BookingRequestDto> Handle(CreateRoomBookingCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(request.userId) ?? throw new NotFoundException("User not found");
                var hotel = await _hotelRepository.FindRoomsAsync(
                    request.HotelId,
                    request.RoomsNumbers
                ) ?? throw new NotFoundException("Hotel not found!");
                foreach (var roomNumber in request.RoomsNumbers)
                {
                    var isRoomExists = hotel.Rooms.Any(r => r.RoomNumber == roomNumber);
                    if (!isRoomExists)
                        throw new NotFoundException($"Room with number '{roomNumber}' not found!");
                    var isRoomAvailable = await _hotelRoomRepository.IsRoomAvailableAsync(
                        request.HotelId,
                        roomNumber,
                        request.StartDate,
                        request.EndDate
                    );
                    if (!isRoomAvailable)
                        throw new ErrorException($"Room with number {roomNumber} not available for selected date")
                        {
                            StatusCode = StatusCodes.Status409Conflict
                        };
                }
                var discountPercentage = await GetDiscountPercentage(request.SpecialOfferId, request.HotelId);

                decimal discountedPricePerNight = hotel.PricePerNight * (1 - ((decimal)discountPercentage / 100));
                var startDate = DateTime.Parse(request.StartDate.ToString());
                var endData = DateTime.Parse(request.EndDate.ToString());
                var daysToStay = (endData - startDate).Days;
                decimal originalTotalPrice = (daysToStay * hotel.PricePerNight) * request.RoomsNumbers.Count();
                decimal discountedTotalPrice = (daysToStay * discountedPricePerNight) * request.RoomsNumbers.Count();

                var booking = new Booking()
                {
                    EndDate = request.EndDate,
                    StartDate = request.StartDate,
                    BookingRooms = request.RoomsNumbers.Select(r => new BookingRoom()
                    {
                        HotelId = hotel.Id,
                        RoomNumber = r
                    }).ToList(),
                    OriginalTotalPrice = originalTotalPrice,
                    DiscountedTotalPrice = discountedTotalPrice,
                    SpecialOfferId = request.SpecialOfferId,
                    UserId = request.userId
                };
                var options = new PaymentIntentCreateOptions
                {
                    Amount = (long)discountedTotalPrice,
                    Currency = "usd",
                    AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                    {
                        Enabled = true,
                    }
                };
                var paymentIntent = await _paymentService.CreateAsync(options, cancellationToken);
                booking.PaymentIntentId = paymentIntent.Id;
                var createdBooking = await _bookingRepository.CreateRoomBookingAsync(booking);
                var bookingDto = _mapper.Map<BookingDto>(createdBooking);

                return new BookingRequestDto()
                {
                    ClientSecret = paymentIntent.ClientSecret,
                    PaymentIntentId = paymentIntent.Id,
                    Booking = bookingDto
                };
            }
            catch (ErrorException)
            {
                throw;
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception exception)
            {
                throw new ErrorException("Error during creating the room booking", exception);
            }
        }
        private async Task<int> GetDiscountPercentage(string? specialOfferId, int hotelId)
        {
            if (string.IsNullOrEmpty(specialOfferId)) return 0;

            var specialOffer = await _specialOfferRepository.GetByIdAsync(specialOfferId)
                ?? throw new NotFoundException(
                    $"The offer with code '{specialOfferId}' not found!");
            if (specialOffer.HotelId != hotelId)
                throw new ErrorException("The offer code is not valid for this hotel.")
                {
                    StatusCode = StatusCodes.Status409Conflict
                };
            if (specialOffer.ExpireDate < DateOnly.FromDateTime(DateTime.UtcNow))
                throw new ErrorException("The offer is expired")
                {
                    StatusCode = StatusCodes.Status410Gone
                };
            return specialOffer.DiscountPercentage;
        }
    }
}
