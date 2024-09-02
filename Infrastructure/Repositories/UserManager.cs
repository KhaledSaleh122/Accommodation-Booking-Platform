using Domain.Abstractions;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Repositories
{
    public class UserManager : IUserManager
    {
        private readonly UserManager<User> userManager;
        public UserManager(UserManager<User> userManager)
        {
            this.userManager = userManager;
        }
        public async Task AddToRoleAsync(User user, string role)
        {
            await userManager.AddToRoleAsync(user, role);
        }

        public async Task<(bool Success, IEnumerable<(string Field, string Description)> Errors)> CreateAsync(User user, string password)
        {
            var result = await userManager.CreateAsync(user, password);
            return (
                result.Succeeded, 
                result.Errors.Select(
                    x => (x.Code, x.Description)
                )
           );
        }

        public async Task<(bool Success, IEnumerable<(string Field, string Description)> Errors)> CreateAsync(User user)
        {
            var result = await userManager.CreateAsync(user);
            return (
                result.Succeeded,
                result.Errors.Select(
                    x => (x.Code, x.Description)
                )
           );
        }

        public async Task<User?> FindByEmailAsync(string email)
        {
            return await userManager.FindByEmailAsync(email);
        }

        public async Task<User?> FindByIdAsync(string userId)
        {
            return await userManager.FindByIdAsync(userId);
        }

        public async Task<User?> FindByNameAsync(string userName)
        {
            return await userManager.FindByNameAsync(userName);
        }

        public async Task<IEnumerable<string>> GetRolesAsync(User user)
        {
            return await userManager.GetRolesAsync(user);
        }

        public async Task UpdateAsync(User user)
        {
            await userManager.UpdateAsync(user);
        }
    }
}
