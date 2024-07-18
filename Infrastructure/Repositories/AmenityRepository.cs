﻿using Domain.Abstractions;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class AmenityRepository : IAmenityRepository
    {
        public readonly ApplicationDbContext _dbContext;
        public AmenityRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<Amenity> DeleteAsync(Amenity amenity)
        {
            _dbContext.Amenities.Remove(amenity);
            await _dbContext.SaveChangesAsync();
            return amenity;
        }

        public async Task<(IEnumerable<Amenity>, uint)> GetAsync(uint page = 1, uint pageSize = 10)
        {
            int totalRecords = await _dbContext.Amenities.CountAsync();
            return (
                     await _dbContext.Amenities
                                   .Take((int) (page * pageSize))
                                   .Skip((int) ((page - 1) * pageSize))
                                   .ToListAsync()
                     , (uint)totalRecords
                   );
        }

        public async Task<Amenity?> GetByIdAsync(uint id)
        {
            return await _dbContext.Amenities.FirstOrDefaultAsync(amenity => amenity.Id == id);
        }

        public async Task CreateAsync(Amenity amenity)
        {
            await _dbContext.AddAsync(amenity);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateAsync(Amenity amenity)
        {
            _dbContext.Entry(amenity).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
        }
    }
}
