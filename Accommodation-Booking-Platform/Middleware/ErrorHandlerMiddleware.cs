﻿using Application.Execptions;
using Presentation.Responses.ServerErrors;

namespace Booking_API_Project.Middleware
{
    public class ErrorHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlerMiddleware> _logger;

        public ErrorHandlerMiddleware(RequestDelegate next, ILogger<ErrorHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);

            }
            catch (ErrorException execption)
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/json";
                var serverErrorResponse = new ServerErrorResponse()
                {
                    Title = execption.Message ?? "An unexpected error occurred",
                    Status = StatusCodes.Status500InternalServerError,
                    TraceId = context.TraceIdentifier
                };
                _logger.LogError($"""
                                 Error Message: {serverErrorResponse.Title}
                                 TraceId: {serverErrorResponse.TraceId}
                                 """);
                await context.Response.WriteAsJsonAsync(serverErrorResponse);
            }
        }
    }
}
