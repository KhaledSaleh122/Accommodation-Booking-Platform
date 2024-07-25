using Domain.Entities;

namespace Domain.Abstractions
{
    public interface IRecentlyVisitedHotelRepository
    {
        public Task AddAsync(RecentlyVisitedHotel recentlyVisitedHotel);
        Task<IDictionary<RecentlyVisitedHotel, double>> GetAsync(string userId);
    }
}
