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

        public async Task UpdateAsync(Room room) {
            _dbContext.Entry(room).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
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

        public async Task<Room?> GetHotelRoomAsync(int hotelId, string roomNumber)
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

        public async Task<bool> IsRoomAvailableAsync(int hotelId, string roomNumber, DateOnly startDate, DateOnly endDate)
        {
            return !(await _dbContext.Bookings.AnyAsync(
                b =>
                    b.StartDate < endDate &&
                    b.EndDate > startDate &&
                    b.BookingRooms.Any(br => br.RoomNumber == roomNumber && br.HotelId == hotelId)
                ));
        }
    }
}
