using Domain.Abstractions;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class RecentlyVisitedHotelRepository : IRecentlyVisitedHotelRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public RecentlyVisitedHotelRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task AddAsync(RecentlyVisitedHotel recentlyVisitedHotel)
        {
            await _dbContext.RecentlyVisitedHotels.AddAsync(recentlyVisitedHotel);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<IEnumerable<RecentlyVisitedHotel>> GetAsync(string userId)
        {
            return await _dbContext.RecentlyVisitedHotels
                .Include(o => o.Hotel).ThenInclude(o => o.City)
                .Include(o => o.Hotel).ThenInclude(o => o.Reviews)
                .Where(rvh => rvh.UserId == userId)
                .OrderByDescending(o => o.VisitedDate)
                .ToListAsync();
        }
    }
}
