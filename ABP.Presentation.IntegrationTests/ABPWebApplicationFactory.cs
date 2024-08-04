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
                
                var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>)
                    );
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                
                services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
                {
                    options.UseInMemoryDatabase(DatabaseName);
                    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
                    options.ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning));
                });
            });
        }

        public async Task SetupDbContext(ApplicationDbContext dbContext) {
            await dbContext.Database.EnsureDeletedAsync();
            await dbContext.Database.EnsureCreatedAsync();
            var roles = new[] { "Admin", "User" };
            foreach (var role in roles)
            {
                await dbContext.Roles.AddAsync(new IdentityRole(role) { Id = "Role"+role });
            }
            var admin = new User { Id = "AdminId", UserName = "Admin", Email = "Admin@gmail.com", Thumbnail = "test.png" };
            await dbContext.Users.AddAsync(admin);
            await dbContext.UserRoles.AddAsync(new IdentityUserRole<string>(){ RoleId = "RoleAdmin",UserId = "AdminId" });

            var user = new User { Id = "UserId", UserName = "User", Email = "User@gmail.com", Thumbnail = "test.png" };
            await dbContext.Users.AddAsync(user);
            await dbContext.UserRoles.AddAsync(new IdentityUserRole<string>(){ RoleId = "RoleUser",UserId = "UserId" });            
            
            var user1 = new User { Id = "User1Id", UserName = "User1", Email = "User1@gmail.com", Thumbnail = "test.png" };
            await dbContext.Users.AddAsync(user1);
            await dbContext.UserRoles.AddAsync(new IdentityUserRole<string>(){ RoleId = "RoleUser",UserId = "User1Id" });
            await dbContext.SaveChangesAsync();
        }

    }
}
