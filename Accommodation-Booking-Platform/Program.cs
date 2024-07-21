using Accommodation_Booking_Platform.Configurations;
using Accommodation_Booking_Platform.Middleware;
using Booking_API_Project.Middleware;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using System.Reflection;

namespace Accommodation_Booking_Platform
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            ConfigureServices(builder.Services, builder.Configuration);

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            Configure(app);

            app.Run();
        }

        private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
                options.SuppressMapClientErrors = true;
            });

            services
                .AddControllers()
                .AddApplicationPart(Assembly.Load("Presentation"));

            services.AddRepositories();
            services.AddDBContextConfiguration(configuration);
            services.AddValidatorsFromAssembly(Assembly.Load("Application"));
            services.AddMediatrConfiguration();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
        }

        private static void Configure(WebApplication app)
        {
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseStaticFiles(
                new StaticFileOptions()
                {
                    FileProvider = new PhysicalFileProvider
                    (
                        Path.Combine(Directory.GetCurrentDirectory(), "Public")
                    ),
                    RequestPath = ""
                }
            );

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.UseMiddleware<NotFoundMiddleware>();
            app.UseMiddleware<ValidationMappingMiddleware>();
            app.UseMiddleware<ErrorHandlerMiddleware>();
            app.MapControllers();
        }
    }
}