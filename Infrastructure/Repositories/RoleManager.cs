using Domain.Abstractions;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Repositories
{
    public class RoleManager : IRoleManager
    {
        private readonly RoleManager<Role> _roleManager;

        public RoleManager(RoleManager<Role> roleManager)
        {
            _roleManager = roleManager;
        }
        public async Task CreateAsync(Role role)
        {
            await _roleManager.CreateAsync(role);
        }

        public async Task<bool> RoleExistsAsync(string role)
        {
            return await _roleManager.RoleExistsAsync(role);
        }
    }
}
