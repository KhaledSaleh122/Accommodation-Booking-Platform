using Application.CommandsAndQueries.AmenityCQ.Commands.Create;
using Application.CommandsAndQueries.AmenityCQ.Commands.Update;
using Application.CommandsAndQueries.CityCQ.Commands.Create;
using Application.CommandsAndQueries.CityCQ.Commands.Update;
using Application.CommandsAndQueries.HotelAmenityCQ.Commands.Create;
using Application.CommandsAndQueries.HotelCQ.Commands.Create;
using System.Reflection;

namespace Accommodation_Booking_Platform.Configurations
{
    public static class AddMediatrSetup
    {
        public static IServiceCollection AddMediatrConfiguration(this IServiceCollection services)
        {
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssemblies(Assembly.Load("Application"));
                cfg.RegisterPreProcessor<CreateAmenityCommand>();
                cfg.RegisterPreProcessor<UpdateAmenityCommand>();
                cfg.RegisterPreProcessor<CreateCityCommand>();
                cfg.RegisterPreProcessor<UpdateCityCommand>();
                cfg.RegisterPreProcessor<CreateHotelCommand>();
                cfg.RegisterPreProcessor<UpdateHotelCommand>();
                cfg.RegisterPreProcessor<AddAmenityToHotelCommand>();
            });
            return services;
        }
    }
}
