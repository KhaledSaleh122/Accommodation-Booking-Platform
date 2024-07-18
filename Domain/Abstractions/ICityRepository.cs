using Domain.Entities;

namespace Domain.Abstractions
{
    public interface ICityRepository
    {
        Task<bool> DoesPostOfficeExistsAsync(string postOffice);
        Task<bool> DoesCityExistInCountryAsync(string cityName,string countryName);
        Task<City> DeleteAsync(City city);
        public Task<(IEnumerable<City>, int)> GetAsync(int page, int pageSize, string? country);
        Task<City?> GetByIdAsync(int cityId);
        Task CreateAsync(City city);
        Task UpdateAsync(City updatedCity);
    }
}
