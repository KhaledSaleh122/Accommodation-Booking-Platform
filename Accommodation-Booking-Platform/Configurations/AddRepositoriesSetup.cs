using Domain.Abstractions;
using Infrastructure.Repositories;

namespace Booking_API_Project.Configurations
{
    public static class AddRepositoriesSetup
    {
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<IAmenityRepository, AmenityRepository>();
            return services;
        }
    }
}
