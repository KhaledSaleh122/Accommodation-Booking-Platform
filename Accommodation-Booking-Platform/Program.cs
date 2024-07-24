using Accommodation_Booking_Platform.Configurations;
using Accommodation_Booking_Platform.Middleware;
using Booking_API_Project.Configurations;
using Booking_API_Project.Middleware;
using Domain.Entities;
using FluentValidation;
using Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text;

namespace Accommodation_Booking_Platform
{
    public class Program
    {
        public async static Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            ConfigureServices(builder.Services, builder.Configuration);

            var app = builder.Build();

            await Configure(app);

            app.Run();
        }

        private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
                options.SuppressMapClientErrors = true;
            });
            services.AddDateOnlyTimeOnlyStringConverters();
            services.AddSwaggerGen(c =>
             {
                c.AddSecurityDefinition("ABPApiBearerAuth", new()
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    Description = "Input a valid token to access this API"
                });
                 c.UseDateOnlyTimeOnlyStringConverters();

                c.AddSecurityRequirement(new()
                {
                    {
                        new ()
                        {
                            Reference = new OpenApiReference {
                                Type = ReferenceType.SecurityScheme,
                                Id = "ABPApiBearerAuth" }
                        },
                        new List<string>()
                    }
                });
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

            services.AddIdentity<User, IdentityRole>(option => {
                option.Password.RequiredLength = 6;
                option.Password.RequireNonAlphanumeric = false;
                option.Password.RequireDigit = true;
                option.Password.RequireLowercase = false;
                option.Password.RequireUppercase = false;
            })
            .AddRoleManager<RoleManager<IdentityRole>>()
            .AddUserManager<UserManager<User>>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            services.AddAuthorization(option => {
                option.AddPolicy("Guest", policy =>
                {
                    policy.RequireAssertion(context =>
                    {
                        return
                        context.User?.Identity is null ||
                        !context.User.Identity.IsAuthenticated;
                    });
                });
                option.AddPolicy("GuestOrUser", policy =>
                {
                    policy.RequireAssertion(context =>
                    {
                        return 
                        context.User?.Identity is null  || 
                        !context.User.Identity.IsAuthenticated ||
                        context.User.IsInRole("User");
                    });
                });
            });

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = false,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration.GetValue<string>("JWTToken:Issuer"),
                    ValidAudience = configuration.GetValue<string>("JWTToken:Audience"),
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(configuration.GetValue<string>("JWTToken:Key")!)
                    )
                };
            });
        }

        private async static Task Configure(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider;
            await services.AddRoleBasedAccessControl(app.Configuration);
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
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseMiddleware<NotFoundMiddleware>();
            app.UseMiddleware<ValidationMappingMiddleware>();
            app.UseMiddleware<ErrorHandlerMiddleware>();
            app.MapControllers();
        }
    }
}