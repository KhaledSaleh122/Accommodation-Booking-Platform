using Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Booking_API_Project.Configurations
{
    public static class RbacConfiguration
    {
        public async static Task<IServiceProvider> AddRoleBasedAccessControl(
            this IServiceProvider services, IConfiguration configuration)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<User>>();
            await EnsureRolesAsync(roleManager);
            var adminName = configuration.GetValue<string>("Admin:Name");
            var adminEmail = configuration.GetValue<string>("Admin:Email");
            var adminPassword = configuration.GetValue<string>("Admin:Password");
            if (adminName is null || adminEmail is null || adminPassword is null)
                throw new Exception("Admin account information is required");
            await CreateAdminAccount(userManager, adminName, adminEmail, adminPassword);
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

        private static async Task CreateAdminAccount(
            UserManager<User> userManager,
            string name,
            string email,
            string password
            ) {
            var user = await userManager.FindByNameAsync(name);
            var user_ = await userManager.FindByEmailAsync(email);
            if (user is null && user_ is not null)
                throw new Exception("There is account with this email");
            if (user is null)
            {
                user = new User { UserName = name, Email = email };
                await userManager.CreateAsync(user, password);
                await userManager.AddToRoleAsync(user, "Admin");
            }
        }


    }
}
