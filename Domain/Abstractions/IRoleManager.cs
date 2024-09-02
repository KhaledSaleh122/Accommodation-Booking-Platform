
using Domain.Entities;

namespace Domain.Abstractions
{
    public interface IRoleManager
    {
        Task CreateAsync(Role role);
        Task<bool> RoleExistsAsync(string role);
    }
}
