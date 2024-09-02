using Accommodation_Booking_Platform.Configurations;
using Accommodation_Booking_Platform.Middleware;
using Asp.Versioning;
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
using Newtonsoft.Json;
using Stripe;
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

            services.AddApiVersioning(options =>
                 {
                     options.DefaultApiVersion = new ApiVersion(1);
                     options.ReportApiVersions = true;
                     options.AssumeDefaultVersionWhenUnspecified = true;
                     options.ApiVersionReader = ApiVersionReader.Combine(
                         new UrlSegmentApiVersionReader(),
                         new HeaderApiVersionReader("X-Api-Version"));
                 }).AddApiExplorer(options =>
                 {
                     options.GroupNameFormat = "'v'V";
                     options.SubstituteApiVersionInUrl = true;
                 });

            services.AddDateOnlyTimeOnlyStringConverters();

            services.AddSwaggerGen(c =>
            {
                var xmlFile = "Presentation.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

                c.IncludeXmlComments(xmlPath);

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
            StripeConfiguration.ApiKey = configuration.GetValue<string>("Stripe:SecretKey");

            services
            .AddControllers()
                .AddNewtonsoftJson(options => options
                    .SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                )
                .AddApplicationPart(Assembly.Load("Presentation"));

            services.AddServices(configuration);
            services.AddDBContextConfiguration(configuration);
            services.AddValidatorsFromAssembly(Assembly.Load("Application"));
            services.AddMediatrConfiguration();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();

            services.AddIdentity<User, Role>(option =>
            {
                option.Password.RequiredLength = 6;
                option.Password.RequireNonAlphanumeric = false;
                option.Password.RequireDigit = true;
                option.Password.RequireLowercase = false;
                option.Password.RequireUppercase = false;
            })
            .AddRoleManager<RoleManager<Role>>()
            .AddUserManager<UserManager<User>>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            services.AddAuthorization(option =>
            {
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
                        context.User?.Identity is null ||
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
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration.GetValue<string>("JWTToken:Issuer"),
                    ValidAudience = configuration.GetValue<string>("JWTToken:Audience"),
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(configuration.GetValue<string>("JWTToken:Key")!)
                    )
                };
            }).AddGoogle(options =>
            {
                options.ClientId = configuration.GetValue<string>("OAuth:ClientID")!;
                options.ClientSecret = configuration.GetValue<string>("OAuth:ClientSecret")!;
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
            string currentDir = Directory.GetCurrentDirectory();
            string staticFolderPath = Path.Combine(currentDir, "Public");
            Directory.CreateDirectory(staticFolderPath);
            app.UseStaticFiles(
                new StaticFileOptions()
                {
                    FileProvider = new PhysicalFileProvider
                    (
                        staticFolderPath
                    ),
                    RequestPath = ""
                }
            );

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseMiddleware<ExceptionHandlerMiddleware>();
            app.UseMiddleware<NotFoundMiddleware>();
            app.UseMiddleware<ValidationMappingMiddleware>();
            app.UseMiddleware<ErrorHandlerMiddleware>();
            app.MapControllers();
        }
    }
}