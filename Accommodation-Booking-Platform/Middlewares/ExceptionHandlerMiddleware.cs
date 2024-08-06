using Booking_API_Project.Middleware;
using Presentation.Responses.ServerErrors;
using System;

namespace Accommodation_Booking_Platform.Middleware
{
    public class ExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlerMiddleware> _logger;

        public ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ErrorHandlerMiddleware> logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);

            }
            catch (Exception exception) 
            {
                context.Response.StatusCode = 500;
                context.Response.ContentType = "application/json";
                var serverErrorResponse = new ServerErrorResponse()
                {
                    Title = "An unexpected error occurred",
                    Status = context.Response.StatusCode,
                    TraceId = context.TraceIdentifier
                };
                string logError = $"""
                                 Error Message: {exception.Message}
                                 TraceId: {serverErrorResponse.TraceId}
                                 Inner Exception Message: {exception.InnerException?.Message}
                                 """;
                _logger.LogError(logError);
                await context.Response.WriteAsJsonAsync(serverErrorResponse);
            }
        }
    }
}
