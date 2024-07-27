using Domain.Abstractions;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class SpecialOfferRepository : ISpecialOfferRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public SpecialOfferRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<SpecialOffer> CreateAsync(SpecialOffer specialOffer)
        {
            await _dbContext.SpecialOffers.AddAsync(specialOffer);
            await _dbContext.SaveChangesAsync();
            return specialOffer;
        }

        public async Task<IEnumerable<SpecialOffer>> FindAsync(int hotelId,IEnumerable<string> specialOffers)
        {
            return await _dbContext.SpecialOffers
                .Where(x => x.HotelId == hotelId &&  specialOffers.Contains(x.Id) && x.ExpireDate >= DateOnly.FromDateTime(DateTime.UtcNow))
                .ToListAsync();
        }

        public async Task<SpecialOffer?> GetByIdAsync(string id)
        {
            return await _dbContext.SpecialOffers
                .FirstOrDefaultAsync(sp => sp.Id == id);
        }

        public async Task<IEnumerable<SpecialOffer>> GetTopSpecialFeatureOffers()
        {
            return await _dbContext.SpecialOffers
                .Include(o => o.Hotel).ThenInclude(o => o.City)
                .Where(
                    sp => 
                        sp.OfferType == OfferType.FeatureDeal &&
                        sp.ExpireDate >= DateOnly.FromDateTime(DateTime.UtcNow) 
                )
                .Take(5)
                .ToListAsync();
        }
    }
}
