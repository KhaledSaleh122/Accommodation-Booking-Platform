using Domain.Entities;

namespace Domain.Abstractions
{
    public interface IHotelRoomRepository
    {
        public Task<bool> RoomNumberExistsAsync(int hotelId, string roomNumber);
        Task AddRoomAsync(Room room);
        Task<Room> DeleteRoomAsync(Room room);
        Task<Room?> GetHotelRoom(int hotelId, string roomNumber);
    }
}
