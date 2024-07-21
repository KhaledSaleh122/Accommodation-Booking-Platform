using Domain.Entities;
using Domain.Enums;

namespace Domain.Abstractions
{
    public interface IHotelRepository
    {
        public Task<bool> RoomNumberExistsAsync(int hotelId, string roomNumber);
        Task AddRoomAsync(Room room);
        Task<Room> DeleteRoomAsync(Room room);
        Task<Room?> GetHotelRoom(int hotelId, string roomNumber);
        Task<Hotel> DeleteAsync(Hotel hotel);
        public Task BeginTransactionAsync();
        public Task CommitTransactionAsync();
        public Task RollbackTransactionAsync();

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
        Task AddAmenityAsync(HotelAmenity amenityHotel);
        Task<bool> AmenityExistsAsync(int hotelId, int amenityId);
        Task RemoveAmenityAsync(HotelAmenity amenityHotel);
    }
}
