using Domain.Abstractions;
using Infrastructure.Repositories;

namespace Accommodation_Booking_Platform.Configurations
{
    public static class AddRepositoriesSetup
    {
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<IAmenityRepository, AmenityRepository>();
            services.AddScoped<ICityRepository, CityRepository>();
            services.AddScoped<IHotelRepository, HotelRepository>();
            services.AddScoped<IImageService, ImageRepository>();
            return services;
        }
    }
}
