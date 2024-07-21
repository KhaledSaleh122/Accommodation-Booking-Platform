using Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Booking_API_Project.Configurations
{
    public static class RbacConfiguration
    {
        public async static Task<IServiceProvider> AddRoleBasedAccessControl(
            this IServiceProvider services)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<User>>();
            await EnsureRolesAsync(roleManager);
            return services;
        }

       private static async Task EnsureRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            var roles = new[] { "Admin", "User" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }


    }
}
