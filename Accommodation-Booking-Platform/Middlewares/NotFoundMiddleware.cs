using Application.Exceptions;
using Presentation.Responses.NotFound;

namespace Accommodation_Booking_Platform.Middleware { 
    public class NotFoundMiddleware
    {
        private readonly RequestDelegate _next;

        public NotFoundMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
                if (context.Response.StatusCode == StatusCodes.Status404NotFound 
                        && !context.Response.HasStarted
                    )
                {
                    ReturnNotFoundResponse(context);
                }
            }
            catch (NotFoundException exception)
            {
                ReturnNotFoundResponse(context, exception.Message);
            }
        }

        public async void ReturnNotFoundResponse(HttpContext context,string? notFoundMessage = null)
        {
            var problemDetails = new NotFoundResponse()
            {
                Status = StatusCodes.Status404NotFound,
                Title = notFoundMessage ?? "Not Found",
                TraceId = context.TraceIdentifier
            };

            context.Response.ContentType = "application/problem+json";
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            await context.Response.WriteAsJsonAsync(problemDetails);
        }
    }
}