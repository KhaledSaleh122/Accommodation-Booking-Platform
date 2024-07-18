using Domain.Entities;

namespace Domain.Abstractions
{
    public interface IAmenityRepository
    {
        public Task CreateAsync(Amenity amenity);
        public Task<Amenity?> GetByIdAsync(uint id);

        public Task<(IEnumerable<Amenity>, uint)> GetAsync(uint page, uint pageSize);

        public Task<Amenity> DeleteAsync(Amenity amenity);

        public Task UpdateAsync(Amenity amenity);

    }
}
