using Domain.Abstractions;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class HotelAmenityRepository : IHotelAmenityRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public HotelAmenityRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task AddAmenityAsync(HotelAmenity amenityHotel)
        {
            await _dbContext.HotelAmenity.AddAsync(amenityHotel);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<bool> AmenityExistsAsync(int hotelId, int amenityId)
        {
            return await _dbContext.HotelAmenity.AnyAsync(
                x =>
                    x.AmenityId == amenityId &&
                    x.HotelId == hotelId
            );
        }

        public async Task RemoveAmenityAsync(HotelAmenity amenityHotel)
        {
            _dbContext.HotelAmenity.Remove(amenityHotel);
            await _dbContext.SaveChangesAsync();
        }
    }
}
