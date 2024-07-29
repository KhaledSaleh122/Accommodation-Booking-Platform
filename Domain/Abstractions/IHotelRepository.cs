using Domain.Entities;
using Domain.Enums;
using Domain.Params;

namespace Domain.Abstractions
{
    public interface IHotelRepository
    {
        Task<Hotel> DeleteAsync(Hotel hotel);

        Task<(IDictionary<Hotel, double>, int)> GetAsync(HotelSearch hotelSearch);
        Task<(Hotel, double)?> GetByIdAsync(int hotelId);
        Task CreateAsync(Hotel hotel);
        Task UpdateAsync(Hotel updatedHotel);
        Task<Hotel?> FindRoomsAsync(int hotelId, IEnumerable<string> roomsIds);
    }
}
