using Application.CommandsAndQueries.AmenityCQ.Commands.Create;
using Application.CommandsAndQueries.AmenityCQ.Commands.Update;
using Application.CommandsAndQueries.CityCQ.Commands.Create;
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
            });
            return services;
        }
    }
}
