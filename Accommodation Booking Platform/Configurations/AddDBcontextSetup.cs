using Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Booking_API_Project.Configurations
{
    public static class AddDBcontextSetup
    {
        public static IServiceCollection AddDBContextConfiguration(this IServiceCollection services, IConfiguration configuration) {
            services.AddDbContext<ApplicationDbContext>(
                options =>
                {
                    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
                    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
                },
                ServiceLifetime.Scoped
            );
            return services;
        }
    }
}
