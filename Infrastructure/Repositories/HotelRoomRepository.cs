using Domain.Abstractions;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class HotelRoomRepository : IHotelRoomRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public HotelRoomRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task AddRoomAsync(Room room)
        {
            await _dbContext.Rooms.AddAsync(room);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<Room> DeleteRoomAsync(Room room)
        {
            _dbContext.Rooms.Remove(room);
            await _dbContext.SaveChangesAsync();
            return room;
        }

        public async Task<Room?> GetHotelRoom(int hotelId, string roomNumber)
        {
            return await _dbContext.Rooms
                .Include(o => o.Images)
                .FirstOrDefaultAsync(
                    p =>
                        p.RoomNumber.ToLower() == roomNumber.ToLower() &&
                        p.HotelId == hotelId
            );
        }

        public async Task<bool> RoomNumberExistsAsync(int hotelId, string roomNumber)
        {
            return await _dbContext.Rooms.AnyAsync(
                p =>
                p.RoomNumber.ToLower() == roomNumber.ToLower() &&
                p.HotelId == hotelId
            );
        }
    }
}
