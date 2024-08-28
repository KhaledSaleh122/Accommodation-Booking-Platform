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
            string id = "Admin";
            var user = await userManager.FindByIdAsync(id);
            var user_ = await userManager.FindByEmailAsync(email);
            if (user_ is not null && user_.Id != id)
                throw new Exception("There is an account with this email");
            var adminUser = new User { Id = id, UserName = name, Email = email, Thumbnail = "admin.jpg" };
            if (user is null)
            {
                await userManager.CreateAsync(adminUser, password);
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
            else {
                await userManager.UpdateAsync(adminUser);
            }
        }


    }
}
