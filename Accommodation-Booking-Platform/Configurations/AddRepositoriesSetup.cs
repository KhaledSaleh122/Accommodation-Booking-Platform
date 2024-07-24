using Domain.Abstractions;
using Infrastructure.Repositories;
using Infrastructure.Services;

namespace Accommodation_Booking_Platform.Configurations
{
    public static class AddRepositoriesSetup
    {
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<IAmenityRepository, AmenityRepository>();
            services.AddScoped<ICityRepository, CityRepository>();
            services.AddScoped<IHotelRepository, HotelRepository>();
            services.AddScoped<IImageService, ImageService>();
            services.AddScoped<IReviewRepository, ReviewRepository>();
            services.AddScoped<IHotelAmenityRepository, HotelAmenityRepository>();
            services.AddScoped<ITransactionService, TransactionService>();
            services.AddScoped<IHotelRoomRepository, HotelRoomRepository>();
            services.AddScoped<IRecentlyVisitedHotelRepository, RecentlyVisitedHotelRepository>();
            services.AddScoped<IBookingRepository, BookingRepository>();

            return services;
        }
    }
}
