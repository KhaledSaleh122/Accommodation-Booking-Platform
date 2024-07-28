using Application.Exceptions;
using FluentValidation;
using Presentation.Responses.Validation;
using System.Text.Json;

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
            catch (CustomValidationException exception)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                var validationFailureResponse = new ValidationFailureResponse()
                {
                    Title = exception.Message,
                    Status = StatusCodes.Status400BadRequest,
                    TraceId = context.TraceIdentifier
                };
                var options = new JsonSerializerOptions
                {
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                };
                await context.Response.WriteAsJsonAsync(validationFailureResponse, options);
            }
            catch (ValidationException exception)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                var validationFailureResponse = new ValidationFailureResponse()
                {
                    Errors = exception.Errors.Select(e => new ValidationResponse()
                    {
                        Name = e.PropertyName,
                        ErrorMessage = e.ErrorMessage
                    }).ToList(),
                    Title = "One or more validation errors occurred.",
                    Status = StatusCodes.Status400BadRequest,
                    TraceId = context.TraceIdentifier
                };
                await context.Response.WriteAsJsonAsync(validationFailureResponse);
            }
        }
    }
}
