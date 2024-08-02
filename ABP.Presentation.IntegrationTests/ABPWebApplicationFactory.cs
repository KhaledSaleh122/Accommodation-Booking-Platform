using Accommodation_Booking_Platform;
using Domain.Entities;
using Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace ABPIntegrationTests
{
    public class ABPWebApplicationFactory : WebApplicationFactory<Program>
    {
        public string DatabaseName { get; set; } = "InMemoryDbForTesting";

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureTestServices(async services =>
            {
                // Remove the existing DbContext registration
                var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>)
                    );
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add DbContext using a unique in-memory database name
                services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
                {
                    options.UseInMemoryDatabase(DatabaseName);
                    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
                    options.ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning));
                });
                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                await SetupDbContext(scope);
            });
        }

        public async Task SetupDbContext(IServiceScope scope) {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await db.Database.EnsureCreatedAsync();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var roles = new[] { "Admin", "User" };
            foreach (var role in roles)
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
            if (await userManager.FindByIdAsync("AdminId") is not null) {
                var admin = new User { Id = "AdminId", UserName = "Admin", Email = "Admin@gmail.com", Thumbnail = "test.png" };
                await userManager.CreateAsync(admin, "admin123456");
                await userManager.AddToRoleAsync(admin, "Admin");
            }
            if (await userManager.FindByIdAsync("UserId") is not null)
            {
                var user = new User { Id = "UserId", UserName = "User", Email = "User@gmail.com", Thumbnail = "test.png" };
                await userManager.CreateAsync(user, "user123456");
                await userManager.AddToRoleAsync(user, "User");
            }
        }

    }
}
