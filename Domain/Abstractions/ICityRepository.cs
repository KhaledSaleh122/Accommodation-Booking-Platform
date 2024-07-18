using Domain.Entities;

namespace Domain.Abstractions
{
    public interface ICityRepository
    {
        Task<bool> CheckPostOfficeExistsAsync(string postOffice);
        Task<City> DeleteAsync(City city);
        public Task<(IEnumerable<City>, int)> GetAsync(int page, int pageSize, string? country);
        Task<City?> GetByIdAsync(int cityId);
        Task CreateAsync(City city);
        Task UpdateAsync(City updatedCity);
    }
}
