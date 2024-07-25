using Domain.Entities;

namespace Domain.Abstractions
{
    public interface IHotelRoomRepository
    {
        public Task<bool> RoomNumberExistsAsync(int hotelId, string roomNumber);
        Task AddRoomAsync(Room room);
        Task<Room> DeleteRoomAsync(Room room);
        Task<Room?> GetHotelRoomAsync(int hotelId, string roomNumber);
        public Task UpdateAsync(Room room);
        Task<bool> IsRoomAvailable(int hotelId, string roomNumber, DateOnly startDate, DateOnly endDate);
    }
}
