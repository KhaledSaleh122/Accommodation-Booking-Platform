using Domain.Abstractions;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;

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

        public async Task<IDictionary<RecentlyVisitedHotel,double>> GetAsync(string userId)
        {
            var result =  await _dbContext.RecentlyVisitedHotels
                .Include(o => o.Hotel).ThenInclude(o => o.City)
                .Include(o => o.Hotel)
                .Where(rvh => rvh.UserId == userId)
                .OrderByDescending(o => o.VisitedDate)
                .Select(o => new { 
                    RecentlyVisitedHotel = o,
                    AvgReviewScore = o.Hotel.Reviews.Count != 0 ? o.Hotel.Reviews.Average(r => r.Rating) : 0
                })
                .ToDictionaryAsync(key => key.RecentlyVisitedHotel, value => value.AvgReviewScore);
            return result;
        }
    }
}
