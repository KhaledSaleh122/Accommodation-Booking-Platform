using Application.Dtos.BookingDtos;
using Application.Exceptions;
using Application.Execptions;
using AutoMapper;
using Domain.Abstractions;
using Domain.Entities;
using MediatR;
using System;

namespace Application.CommandsAndQueries.BookingCQ.Queries.GetUserbookingById
{
    public class GetUserBookingByIdHandler : IRequestHandler<GetUserBookingByIdQuery, BookingDto>
    {
        private readonly IMapper _mapper;
        private readonly IBookingRepository _bookingRepository;

        public GetUserBookingByIdHandler(IBookingRepository bookingRepository)
        {
            _bookingRepository = bookingRepository ?? throw new ArgumentNullException(nameof(bookingRepository));
            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Booking, BookingDto>()
                    .ForMember(dest => dest.HotelId,
                        opt => opt.MapFrom(src => src.BookingRooms.First().HotelId))
                    .ForMember(dest => dest.Rooms,
                        opt => opt.MapFrom(src => src.BookingRooms.Select(x => x.RoomNumber).ToList())); ;
            });
            _mapper = configuration.CreateMapper();
        }

        public async Task<BookingDto> Handle(GetUserBookingByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var booking = await _bookingRepository.GetByIdAsync(request.UserId, request.BookingId) 
                    ?? throw new NotFoundException("Booking not found!");
                return _mapper.Map<BookingDto>(booking);

            }
            catch (NotFoundException) {
                throw;
            }
            catch (Exception exception)
            {

                throw new ErrorException($"Error during Getting the booking.", exception);
            }
        }
    }
}
