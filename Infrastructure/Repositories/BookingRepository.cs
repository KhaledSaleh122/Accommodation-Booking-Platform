using Domain.Abstractions;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class BookingRepository : IBookingRepository
    {
        private readonly ApplicationDbContext _dbContext;
        public BookingRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<Booking> CreateRoomBookingAsync(Booking booking)
        {
            await _dbContext.Bookings.AddAsync(booking);
            await _dbContext.SaveChangesAsync();
            return booking;
        }

        public async Task<(IEnumerable<Booking>,int)> GetAsync(
            string userId, int page, int pageSize, DateOnly? startDate, DateOnly? endDate)
        {
            var query =  _dbContext.Bookings
                .Include(o => o.BookingRooms)
                .Where(b => b.UserId == userId);
            if(startDate is not null)
                query = query.Where(b => b.StartDate >= startDate);
            if (endDate is not null)
                query = query.Where(b => b.EndDate <= endDate);
            int totalRecords = await query.CountAsync();
            return (
                await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(),
                totalRecords);
        }

        public async Task<Booking?> GetByIdAsync(string userId, int bookingId)
        {
            return await _dbContext.Bookings
                .Include(o => o.BookingRooms)
                .FirstOrDefaultAsync(b => b.UserId == userId && b.Id == bookingId);
        }

        public async Task<Booking?> GetByPaymentIntentIdAsync(string paymentIntentId)
        {
            return await _dbContext.Bookings
                 .Include(o => o.BookingRooms)
                 .FirstOrDefaultAsync(b => b.PaymentIntentId == paymentIntentId);
        }
    }
}
