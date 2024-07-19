using Domain.Abstractions;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class CityRepository : ICityRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public CityRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<bool> DoesPostOfficeExistsAsync(string postOffice)
        {
            return await _dbContext.Cities.AnyAsync(x => x.PostOffice == postOffice);
        }

        public async Task<bool> DoesCityExistInCountryAsync(string cityName, string countryName)
        {
            return await _dbContext.Cities.AnyAsync(
                x => 
                    x.Name.ToLower() == cityName.ToLower() &&
                    x.Country.ToLower() == countryName.ToLower()
            );
        }

        public async Task<City> DeleteAsync(City city)
        {
            _dbContext.Cities.Remove(city);
            await _dbContext.SaveChangesAsync();
            return city;
        }

        public async Task<(IEnumerable<City>, int)> GetAsync(int page, int pageSize, string? country, string? city)
        {
            IQueryable<City> query = _dbContext.Cities;
            if(!string.IsNullOrEmpty(country)) query = query.Where(x => x.Country.Contains(country));
            if(!string.IsNullOrEmpty(city)) query = query.Where(x => x.Name.Contains(city));
            int totalRecords =  await query.CountAsync();
            return (
                     await query
                        .Take(page * pageSize)
                        .Skip((page - 1) * pageSize)
                        .ToListAsync()
                     , totalRecords
                   );
        }

        public async Task<City?> GetByIdAsync(int cityId)
        {
            return await _dbContext.Cities.FirstOrDefaultAsync(city => city.Id == cityId);
        }

        public async Task CreateAsync(City city)
        {
            await _dbContext.AddAsync(city);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateAsync(City updatedCity)
        {
            _dbContext.Entry(updatedCity).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
        }
    }
}
