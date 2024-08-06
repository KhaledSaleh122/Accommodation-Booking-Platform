using Application.Execptions;
using Presentation.Responses.ServerErrors;
using Application.Exceptions;
namespace Booking_API_Project.Middleware
{
    public class ErrorHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlerMiddleware> _logger;

        public ErrorHandlerMiddleware(RequestDelegate next, ILogger<ErrorHandlerMiddleware> logger)
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
            catch (ErrorException exception)
            {
                context.Response.StatusCode = exception.StatusCode ?? StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/json";
                var serverErrorResponse = new ServerErrorResponse()
                {
                    Title = exception.Message ?? "An unexpected error occurred",
                    Status = context.Response.StatusCode,
                    TraceId = context.TraceIdentifier
                };
                string logError = $"""
                                 Error Message: {serverErrorResponse.Title}
                                 TraceId: {serverErrorResponse.TraceId}
                                 Inner Exception Message: {exception.InnerException?.Message}
                                 """;
                if (exception.LoggerLevel == LoggerLevel.Critical)
                {
                    _logger.LogCritical(logError);
                }
                else
                {
                    _logger.LogError(logError);
                }
                await context.Response.WriteAsJsonAsync(serverErrorResponse);
            }
        }
    }
}
