using Domain.Entities;
using Domain.Enums;

namespace Domain.Abstractions
{
    public interface IHotelRepository
    {
        Task<Hotel> DeleteAsync(Hotel hotel);

        public Task<(IEnumerable<Hotel>, int)> GetAsync
            (
                int page,
                int pageSize,
                decimal minPrice,
                decimal? maxPrice,
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
    }
}
