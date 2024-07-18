using Booking_API_Project.Configurations;
using System.Reflection;
using FluentValidation;
using Booking_API_Project.Middleware;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.Configure<ApiBehaviorOptions>(
    options =>
    {
        options.SuppressModelStateInvalidFilter = true;
        options.SuppressMapClientErrors = true;
    }
);

builder.Services
    .AddControllers()
    .AddApplicationPart(Assembly.Load("Presentation"));

builder.Services.AddRepositories();
builder.Services.AddDBContextConfiguration(builder.Configuration);
builder.Services.AddValidatorsFromAssembly(Assembly.Load("Application"));
builder.Services.AddMediatrConfiguration();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseMiddleware<NotFoundMiddleware>();

app.MapControllers();

app.Run();
