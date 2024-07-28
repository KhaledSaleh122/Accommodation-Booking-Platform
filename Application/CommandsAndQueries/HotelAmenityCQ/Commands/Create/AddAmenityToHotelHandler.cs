using Application.Exceptions;
using Application.Execptions;
using Domain.Abstractions;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.CommandsAndQueries.HotelAmenityCQ.Commands.Create
{
    internal class AddAmenityToHotelHandler : IRequestHandler<AddAmenityToHotelCommand>
    {
        private readonly IHotelRepository _hotelRepository;
        private readonly IAmenityRepository _amenityRepository;
        private readonly IHotelAmenityRepository _hotelAmenityRepository;

        public AddAmenityToHotelHandler(
                IAmenityRepository amenityRepository,
                IHotelRepository hotelRepository, 
                IHotelAmenityRepository hotelAmenityRepository
            )
        {
            _amenityRepository = amenityRepository ?? throw new ArgumentNullException(nameof(amenityRepository));
            _hotelRepository = hotelRepository ?? throw new ArgumentNullException(nameof(hotelRepository));
            _hotelAmenityRepository = hotelAmenityRepository ?? throw new ArgumentNullException(nameof(hotelAmenityRepository));
        }

        public async Task Handle(AddAmenityToHotelCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var hotel = await _hotelRepository.GetByIdAsync(request.HotelId)
                    ?? throw new NotFoundException("Hotel not found");
                var amenity = await _amenityRepository.GetByIdAsync(request.AmenityId)
                    ?? throw new NotFoundException("Amenity not found");
                var amenityHotel = new HotelAmenity()
                {
                    AmenityId = request.AmenityId,
                    HotelId = request.HotelId
                };
                var isAmenityExist = await _hotelAmenityRepository.AmenityExistsAsync(request.HotelId, request.AmenityId);
                if(isAmenityExist)
                    throw new ErrorException("The hotel already has this amenity.")
                    {
                        StatusCode = StatusCodes.Status409Conflict
                    };
                await _hotelAmenityRepository.AddAmenityAsync(amenityHotel);
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception exception)
            {
                throw new ErrorException($"Error during adding the amenity to the hotel.", exception);
            }
        }
    }
}
