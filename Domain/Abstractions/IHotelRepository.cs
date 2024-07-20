using Domain.Entities;
using Domain.Enums;

namespace Domain.Abstractions
{
    public interface IHotelRepository
    {
        Task<Hotel> DeleteAsync(Hotel hotel);
        public Task BeginTransactionAsync();
        public Task CommitTransactionAsync();
        public Task RollbackTransactionAsync();

        public Task<(IEnumerable<Hotel>, int)> GetAsync
            (
                int page,
                int pageSize,
                int minPrice,
                int? maxPrice,
                string? city,
                string? country,
                HotelType[] hotelType,
                string? hotelName,
                string? owner,
                int[] aminites
            );
        Task<Hotel?> GetByIdAsync(int hotelId);
        Task CreateAsync(Hotel hotel);
        Task UpdateAsync(Hotel updatedHotel);
        Task AddAmenityAsync(HotelAmenity amenityHotel);
        Task<bool> AmenityExistsAsync(int hotelId, int amenityId);
        Task RemoveAmenityAsync(HotelAmenity amenityHotel);
    }
}
