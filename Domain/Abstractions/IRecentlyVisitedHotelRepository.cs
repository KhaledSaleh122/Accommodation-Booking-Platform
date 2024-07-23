using Domain.Entities;

namespace Domain.Abstractions
{
    public interface IRecentlyVisitedHotelRepository
    {
        public Task AddAsync(RecentlyVisitedHotel recentlyVisitedHotel);
    }
}
