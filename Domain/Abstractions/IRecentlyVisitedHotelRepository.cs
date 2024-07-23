using Domain.Entities;

namespace Domain.Abstractions
{
    public interface IRecentlyVisitedHotelRepository
    {
        public Task AddAsync(RecentlyVisitedHotel recentlyVisitedHotel);
        Task<IEnumerable<RecentlyVisitedHotel>> GetAsync(string userId);
    }
}
