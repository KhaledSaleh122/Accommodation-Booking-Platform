using Microsoft.AspNetCore.Mvc.Testing;
using Accommodation_Booking_Platform;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Infrastructure;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
namespace ABPIntegrationTests
{
    public class ABPWebApplicationFactory : WebApplicationFactory<Program>
    {
        private readonly UserManager<User> userManager;

        public ABPWebApplicationFactory(UserManager<User> userManager)
        {
            this.userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureTestServices(async services =>
            {
                // Remove the existing DbContext registration
                var descriptor = services.SingleOrDefault(
                        d => 
                        d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>)
                    );
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add DbContext using in-memory database
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase("InMemoryDbForTesting");
                    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
                    options.ConfigureWarnings(options => options.Ignore(InMemoryEventId.TransactionIgnoredWarning));
                });

                // Ensure database is created
                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Database.EnsureCreated();
                await userManager.CreateAsync(new User { UserName = "test", Email = "test@gmail.com" }, "test123");
            });
        }
    }
}
