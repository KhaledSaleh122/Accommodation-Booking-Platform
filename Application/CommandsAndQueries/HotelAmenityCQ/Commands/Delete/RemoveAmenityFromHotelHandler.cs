using Application.Exceptions;
using Application.Execptions;
using Domain.Abstractions;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.CommandsAndQueries.HotelAmenityCQ.Commands.Delete
{
    internal class RemoveAmenityFromHotelHandler : IRequestHandler<RemoveAmenityFromHotelCommand>
    {
        private readonly IHotelRepository _hotelRepository;
        private readonly IAmenityRepository _amenityRepository;
        private readonly IHotelAmenityRepository _hotelAmenityRepository;

        public RemoveAmenityFromHotelHandler(
            IAmenityRepository amenityRepository,
            IHotelRepository hotelRepository,
            IHotelAmenityRepository hotelAmenityRepository)
        {
            _amenityRepository = amenityRepository ?? throw new ArgumentNullException(nameof(amenityRepository));
            _hotelRepository = hotelRepository ?? throw new ArgumentNullException(nameof(hotelRepository));
            _hotelAmenityRepository = hotelAmenityRepository ?? throw new ArgumentNullException(nameof(hotelAmenityRepository));
        }

        public async Task Handle(RemoveAmenityFromHotelCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var hotel = await _hotelRepository.GetByIdAsync(request.HotelId)
                    ?? throw new NotFoundException("Hotel not found");
                var amenity = await _amenityRepository.GetByIdAsync(request.AmenityId)
                    ?? throw new NotFoundException("Amenity not found");
                var isAmenityExist = await _hotelAmenityRepository.AmenityExistsAsync(request.HotelId, request.AmenityId);
                if (!isAmenityExist)
                    throw new ErrorException("The hotel doesn’t have this amenity.")
                    {
                        StatusCode = StatusCodes.Status409Conflict
                    };
                var amenityHotel = new HotelAmenity()
                {
                    AmenityId = request.AmenityId,
                    HotelId = request.HotelId
                };
                await _hotelAmenityRepository.RemoveAmenityAsync(amenityHotel);
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (ErrorException) {
                throw;
            }
            catch (Exception exception)
            {
                throw new ErrorException($"Error during remove the amenity from the hotel.", exception);
            }
        }
    }
}
