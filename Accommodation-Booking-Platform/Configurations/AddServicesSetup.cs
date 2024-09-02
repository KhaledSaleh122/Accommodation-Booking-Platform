

using Domain.Abstractions;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Stripe;
using System.Net;
using System.Net.Mail;

namespace Accommodation_Booking_Platform.Configurations
{
    public static class AddServicesSetup
    {
        public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IAmenityRepository, AmenityRepository>();
            services.AddScoped<ICityRepository, CityRepository>();
            services.AddScoped<IHotelRepository, HotelRepository>();
            services.AddScoped<IImageService, ImageService>();
            services.AddScoped<IHotelAmenityRepository, HotelAmenityRepository>();
            services.AddScoped<ITransactionService, TransactionService>();
            services.AddScoped<IHotelRoomRepository, HotelRoomRepository>();
            services.AddScoped<IRecentlyVisitedHotelRepository, RecentlyVisitedHotelRepository>();
            services.AddScoped<IBookingRepository, BookingRepository>();
            services.AddScoped<ISpecialOfferRepository, SpecialOfferRepository>();
            services.AddScoped<IReviewHotelRepository, ReviewRepository>();
            services.AddScoped<PaymentIntentService, PaymentIntentService>();
            services.AddScoped<SmtpClient>(serviceProvider =>
            {
                var smtpClient = new SmtpClient
                {
                    Host = configuration["Smtp:Host"]!,
                    Port = int.Parse(configuration["Smtp:Port"]!),
                    Credentials = new NetworkCredential(
                        configuration["Smtp:Email"],
                        configuration["Smtp:AppPassword"]
                    )
                };
                return smtpClient;
            });

            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IPaymentService<PaymentIntent, PaymentIntentCreateOptions>, StripePaymentService>();
            services.AddScoped<ITokenService,Infrastructure.Services.TokenService>();
            services.AddScoped<IInvoiceGeneraterService,InvoiceGeneraterService>();
            services.AddScoped<IUserManager,UserManager>();
            services.AddScoped<ISignInManager,SignInManager>();
            services.AddScoped<IRoleManager,RoleManager>();

            return services;
        }
    }
}
