using Application.Dtos.BookingDtos;
using Application.Exceptions;
using Application.Execptions;
using AutoMapper;
using Domain.Abstractions;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;
using System;

namespace Application.CommandsAndQueries.BookingCQ.Commands.Create
{
    public class CreateRoomBookingHandler : IRequestHandler<CreateRoomBookingCommand, BookingDto>
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IHotelRepository _hotelRepository;
        private readonly IHotelRoomRepository _hotelRoomRepository;
        private readonly IMapper _mapper;

        public CreateRoomBookingHandler(
            IBookingRepository bookingRepository,
            IHotelRoomRepository hotelRoomRepository,
            ITransactionService transactionService,
            IHotelRepository hotelRepository)
        {
            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Booking, BookingDto>();
                cfg.CreateMap<CreateRoomBookingCommand, Booking>();
            });
            _mapper = configuration.CreateMapper();
            _bookingRepository = bookingRepository ?? throw new ArgumentNullException(nameof(bookingRepository));
            _hotelRoomRepository = hotelRoomRepository ?? throw new ArgumentNullException(nameof(hotelRoomRepository));
            _hotelRepository = hotelRepository ?? throw new ArgumentNullException(nameof(hotelRepository));
        }

        public async Task<BookingDto> Handle(CreateRoomBookingCommand request, CancellationToken cancellationToken)
        {
            var booking = _mapper.Map<Booking>(request);
            booking.HotelId = request.hotelId;
            booking.UserId = request.userId;
            booking.RoomNumber = request.roomNumber;
            try
            {
                var hotel = await _hotelRepository.GetByIdAsync(request.hotelId) 
                    ?? throw new NotFoundException("Hotel not found");
                var room = await _hotelRoomRepository.GetHotelRoomAsync(request.hotelId, request.roomNumber)
                    ?? throw new NotFoundException("Room not found");
                var isRoomAvailable = await _hotelRoomRepository.IsRoomAvailable(
                        request.hotelId,
                        request.roomNumber,
                        request.StartDate,
                        request.EndDate
                    );
                if (!isRoomAvailable)
                    throw new ErrorException("Room not available for selected date")
                    {
                        StatusCode = StatusCodes.Status409Conflict
                    };
                var createdBooking = await _bookingRepository.CreateRoomBookingAsync(booking);
                await _hotelRoomRepository.UpdateAsync(room);
                return _mapper.Map<BookingDto>(createdBooking);

            }
            catch (ErrorException) {
                throw;
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception exception)
            {
                throw new ErrorException("Error during creating the room booking ", exception);
            }
        }
    }
}
