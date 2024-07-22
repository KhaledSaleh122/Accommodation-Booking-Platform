using Application.CommandsAndQueries.AmenityCQ.Commands.Create;
using Application.CommandsAndQueries.AmenityCQ.Commands.Update;
using Application.CommandsAndQueries.CityCQ.Commands.Create;
using Application.CommandsAndQueries.CityCQ.Commands.Update;
using Application.CommandsAndQueries.HotelAmenityCQ.Commands.Create;
using Application.CommandsAndQueries.HotelCQ.Commands.Create;
using Application.CommandsAndQueries.ReviewCQ.Commands.Create;
using Application.CommandsAndQueries.RoomCQ.Commands.Create;
using Application.CommandsAndQueries.UserCQ.Commands.Create;
using Application.CommandsAndQueries.UserCQ.Commands.SignIn;
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
                cfg.RegisterPreProcessor<CreateRoomCommand>();
                cfg.RegisterPreProcessor<CreateUserCommand>();
                cfg.RegisterPreProcessor<SignInUserCommand>();
                cfg.RegisterPreProcessor<CreateReviewCommand>();
            });
            return services;
        }
    }
}
