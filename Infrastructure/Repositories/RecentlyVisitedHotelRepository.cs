using Domain.Abstractions;
using Domain.Entities;

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
    }
}
