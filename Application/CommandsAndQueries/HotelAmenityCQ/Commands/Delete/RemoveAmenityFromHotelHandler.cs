using Application.Exceptions;
using Application.Execptions;
using Domain.Abstractions;
using Domain.Entities;
using FluentValidation;
using MediatR;

namespace Application.CommandsAndQueries.HotelAmenityCQ.Commands.Delete
{
    public class RemoveAmenityFromHotelHandler : IRequestHandler<RemoveAmenityFromHotelCommand>
    {
        private readonly IHotelRepository _hotelRepository;
        private readonly IAmenityRepository _amenityRepository;

        public RemoveAmenityFromHotelHandler(IAmenityRepository amenityRepository, IHotelRepository hotelRepository)
        {
            _amenityRepository = amenityRepository ?? throw new ArgumentNullException(nameof(amenityRepository));
            _hotelRepository = hotelRepository ?? throw new ArgumentNullException(nameof(hotelRepository));
        }

        public async Task Handle(RemoveAmenityFromHotelCommand request, CancellationToken cancellationToken)
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
                await _hotelRepository.RemoveAmenityAsync(amenityHotel);
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception exception)
            {
                throw new ErrorException($"Error during remove the amenity from the hotel.", exception);
            }
        }
    }
}
