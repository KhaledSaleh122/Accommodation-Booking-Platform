using Domain.Entities;
using Domain.Enums;

namespace Domain.Abstractions
{
    public interface IHotelRepository
    {
        Task<Hotel> DeleteAsync(Hotel hotel);

        Task<(IDictionary<Hotel, double>, int)> GetAsync
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
        Task<(Hotel, double)?> GetByIdAsync(int hotelId);
        Task CreateAsync(Hotel hotel);
        Task UpdateAsync(Hotel updatedHotel);
    }
}
