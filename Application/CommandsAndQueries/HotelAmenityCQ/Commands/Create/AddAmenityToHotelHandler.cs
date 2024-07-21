using Application.Exceptions;
using Application.Execptions;
using Domain.Abstractions;
using Domain.Entities;
using MediatR;

namespace Application.CommandsAndQueries.HotelAmenityCQ.Commands.Create
{
    public class AddAmenityToHotelHandler : IRequestHandler<AddAmenityToHotelCommand>
    {
        private readonly IHotelRepository _hotelRepository;
        private readonly IAmenityRepository _amenityRepository;

        public AddAmenityToHotelHandler(IAmenityRepository amenityRepository, IHotelRepository hotelRepository)
        {
            _amenityRepository = amenityRepository ?? throw new ArgumentNullException(nameof(amenityRepository));
            _hotelRepository = hotelRepository ?? throw new ArgumentNullException(nameof(hotelRepository));
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
                await _hotelRepository.AddAmenityAsync(amenityHotel);
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
