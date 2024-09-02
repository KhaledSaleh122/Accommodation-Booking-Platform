
using Domain.Entities;

namespace Domain.Abstractions
{
    public interface IUserManager
    {
        Task AddToRoleAsync(User user, string role);
        Task<(bool Success, IEnumerable<(string Field,string Description)> Errors)> CreateAsync(User user, string password);
        Task<(bool Success, IEnumerable<(string Field, string Description)> Errors)> CreateAsync(User user);
        Task<User?> FindByEmailAsync(string email);
        Task<User?> FindByIdAsync(string userId);
        Task<User?> FindByNameAsync(string userName);
        Task<IEnumerable<string>> GetRolesAsync(User user);
        Task UpdateAsync(User user);
    }
}
