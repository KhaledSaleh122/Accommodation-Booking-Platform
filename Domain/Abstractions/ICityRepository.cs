using Domain.Entities;

namespace Domain.Abstractions
{
    public interface ICityRepository
    {
        Task<bool> DoesPostOfficeExistsAsync(string postOffice);
        Task<bool> DoesCityExistInCountryAsync(string cityName,string countryName);
        Task<City> DeleteAsync(City city);
        public Task<(IEnumerable<City>, uint)> GetAsync(uint page, uint pageSize, string? country,string? city);
        Task<City?> GetByIdAsync(uint cityId);
        Task CreateAsync(City city);
        Task UpdateAsync(City updatedCity);
    }
}
