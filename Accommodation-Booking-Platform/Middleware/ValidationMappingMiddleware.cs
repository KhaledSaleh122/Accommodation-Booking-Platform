using FluentValidation;
using Presentation.Responses.Validation;

namespace Accommodation_Booking_Platform.Middleware
{
    public class ValidationMappingMiddleware
    {
        private readonly RequestDelegate _next;

        public ValidationMappingMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);

            }
            catch (ValidationException exception)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                context.Response.ContentType = "application/json";
                var validationFailureResponse = new ValidationFailureResponse()
                {
                    Errors = exception.Errors.Select(e => new ValidationResponse() { Name = e.PropertyName, ErrorMessage = e.ErrorMessage }).ToList(),
                    Title = exception.InnerException?.Message ?? "One or more validation errors occurred.",
                    Status = StatusCodes.Status400BadRequest,
                    TraceId = context.TraceIdentifier
                };
                await context.Response.WriteAsJsonAsync(validationFailureResponse);
            }
        }
    }
}
