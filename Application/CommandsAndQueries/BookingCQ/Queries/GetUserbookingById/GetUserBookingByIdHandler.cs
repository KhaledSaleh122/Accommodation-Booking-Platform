using Application.Dtos.BookingDtos;
using Application.Exceptions;
using Application.Execptions;
using AutoMapper;
using Domain.Abstractions;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Stripe;

namespace Application.CommandsAndQueries.BookingCQ.Queries.GetUserbookingById
{
    internal class GetUserBookingByIdHandler : IRequestHandler<GetUserBookingByIdQuery, BookingWithPaymentIntentDto>
    {
        private readonly IMapper _mapper;
        private readonly IBookingRepository _bookingRepository;
        private readonly IPaymentService<PaymentIntent, PaymentIntentCreateOptions> _paymentService;
        private readonly UserManager<User> _userManager;

        public GetUserBookingByIdHandler(IBookingRepository bookingRepository,
            IPaymentService<PaymentIntent, PaymentIntentCreateOptions> paymentService,
            UserManager<User> userManager)
        {
            _bookingRepository = bookingRepository ?? throw new ArgumentNullException(nameof(bookingRepository));
            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Booking, BookingWithPaymentIntentDto>()
                    .ForMember(dest => dest.HotelId,
                        opt => opt.MapFrom(src => src.BookingRooms.First().HotelId))
                    .ForMember(dest => dest.Rooms,
                        opt => opt.MapFrom(src => src.BookingRooms.Select(x => x.RoomNumber).ToList())); ;
            });
            _mapper = configuration.CreateMapper();
            _paymentService = paymentService ?? throw new ArgumentNullException(nameof(paymentService));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        public async Task<BookingWithPaymentIntentDto> Handle(GetUserBookingByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(request.UserId) ?? throw new NotFoundException("User not found!");
                var booking = await _bookingRepository.GetByIdAsync(request.UserId, request.BookingId)
                    ?? throw new NotFoundException("Booking not found!");
                var paymentIntent = await _paymentService.GetAsync(booking.PaymentIntentId,cancellationToken)
                    ?? throw new ErrorException(
                        "We encountered an issue while processing your payment. Please contact customer support.")
                    {
                        StatusCode = 500
                    };

                var bookingDto = _mapper.Map<BookingWithPaymentIntentDto>(booking);
                bookingDto.PaymentIntentId = paymentIntent.Id;
                bookingDto.PaymentIntentStatus = paymentIntent.Status;
                bookingDto.ClientSecret = paymentIntent.ClientSecret;
                return bookingDto;
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception exception)
            {

                throw new ErrorException($"Error during Getting the booking.", exception);
            }
        }
    }
}
