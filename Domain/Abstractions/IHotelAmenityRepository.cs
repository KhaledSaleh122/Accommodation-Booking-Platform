using Domain.Entities;

namespace Domain.Abstractions
{
    public interface IHotelAmenityRepository
    {
        Task AddAmenityAsync(HotelAmenity amenityHotel);
        Task<bool> AmenityExistsAsync(int hotelId, int amenityId);
        Task RemoveAmenityAsync(HotelAmenity amenityHotel);
    }
}
