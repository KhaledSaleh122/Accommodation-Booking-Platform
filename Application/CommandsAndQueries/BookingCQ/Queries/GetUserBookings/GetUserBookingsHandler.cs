using Application.Dtos.BookingDtos;
using Application.Execptions;
using AutoMapper;
using Domain.Abstractions;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;

namespace Application.CommandsAndQueries.BookingCQ.Queries.GetUserBookings
{
    public class GetUserBookingsHandler : IRequestHandler<GetUserBookingsQuery,(IEnumerable<BookingDto>, int, int, int)>
    {
        private readonly IMapper _mapper;
        private readonly IBookingRepository _bookingRepository;

        public GetUserBookingsHandler(IBookingRepository bookingRepository)
        {
            _bookingRepository = bookingRepository ?? throw new ArgumentNullException(nameof(bookingRepository));
            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Booking, BookingDto>()
                    .ForMember(dest => dest.HotelId,
                        opt => opt.MapFrom(src => src.BookingRooms.First().HotelId))
                    .ForMember(dest => dest.Rooms,
                        opt => opt.MapFrom(src => src.BookingRooms.Select(x => x.RoomNumber).ToList()));;
            });
            _mapper = configuration.CreateMapper();
        }

        public async Task<(IEnumerable<BookingDto>, int, int, int )> Handle(GetUserBookingsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var (bookings, totalRecords) = await _bookingRepository.GetAsync(
                     request.UserId,
                     request.Page,
                     request.PageSize,
                     request.StartDate,
                     request.EndDate
                );
                return (_mapper.Map<IEnumerable<BookingDto>>(bookings), totalRecords, request.Page,request.PageSize);
            }
            catch (Exception exception)
            {

                throw new ErrorException("Error during getting the user bookings ", exception);
            }
        }
    }
}
