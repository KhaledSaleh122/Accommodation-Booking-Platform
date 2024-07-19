using Domain.Entities;

namespace Domain.Abstractions
{
    public interface IAmenityRepository
    {
        public Task CreateAsync(Amenity amenity);
        public Task<Amenity?> GetByIdAsync(int id);

        public Task<(IEnumerable<Amenity>, int)> GetAsync(int page, int pageSize);

        public Task<Amenity> DeleteAsync(Amenity amenity);

        public Task UpdateAsync(Amenity amenity);

    }
}
