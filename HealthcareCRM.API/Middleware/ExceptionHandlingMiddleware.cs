using System.Net;
using System.Text.Json;

namespace HealthcareCRM.API.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                context.Response.ContentType = "application/json";

                var (statusCode, message) = ex switch
                {
                    ArgumentException => (HttpStatusCode.BadRequest, ex.Message),
                    KeyNotFoundException => (HttpStatusCode.NotFound, "Resource not found."),
                    UnauthorizedAccessException => (HttpStatusCode.Forbidden, "Access denied."),
                    _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred. Please try again.")
                };

                if (statusCode == HttpStatusCode.InternalServerError)
                    _logger.LogError(ex, "Unhandled exception");
                else
                    _logger.LogWarning(ex, "Handled exception: {Message}", ex.Message);

                context.Response.StatusCode = (int)statusCode;

                var result = JsonSerializer.Serialize(new
                {
                    success = false,
                    message,
                    data = (object?)null
                });

                await context.Response.WriteAsync(result);
            }
        }
    }
}