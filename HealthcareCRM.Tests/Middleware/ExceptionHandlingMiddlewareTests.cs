using System.Net;
using System.Text.Json;
using HealthcareCRM.API.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace HealthcareCRM.Tests.Middleware
{
    public class ExceptionHandlingMiddlewareTests
    {
        private static (HttpContext context, Mock<ILogger<ExceptionHandlingMiddleware>> logger) CreateContext()
        {
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();
            var logger = new Mock<ILogger<ExceptionHandlingMiddleware>>();
            return (context, logger);
        }

        private static async Task<(int statusCode, JsonElement body)> InvokeAndReadAsync(
            ExceptionHandlingMiddleware middleware, HttpContext context)
        {
            await middleware.InvokeAsync(context);

            context.Response.Body.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(context.Response.Body);
            var text = await reader.ReadToEndAsync();
            var json = JsonDocument.Parse(text).RootElement;

            return (context.Response.StatusCode, json);
        }

        [Fact]
        public async Task ArgumentException_Returns400()
        {
            var (context, logger) = CreateContext();
            RequestDelegate next = _ => throw new ArgumentException("Invalid email format.");
            var middleware = new ExceptionHandlingMiddleware(next, logger.Object);

            var (statusCode, body) = await InvokeAndReadAsync(middleware, context);

            Assert.Equal((int)HttpStatusCode.BadRequest, statusCode);
            Assert.False(body.GetProperty("success").GetBoolean());
            Assert.Equal("Invalid email format.", body.GetProperty("message").GetString());
        }

        [Fact]
        public async Task KeyNotFoundException_Returns404()
        {
            var (context, logger) = CreateContext();
            RequestDelegate next = _ => throw new KeyNotFoundException("Patient not found.");
            var middleware = new ExceptionHandlingMiddleware(next, logger.Object);

            var (statusCode, body) = await InvokeAndReadAsync(middleware, context);

            Assert.Equal((int)HttpStatusCode.NotFound, statusCode);
            Assert.Equal("Resource not found.", body.GetProperty("message").GetString());
        }

        [Fact]
        public async Task UnauthorizedAccessException_Returns403()
        {
            var (context, logger) = CreateContext();
            RequestDelegate next = _ => throw new UnauthorizedAccessException();
            var middleware = new ExceptionHandlingMiddleware(next, logger.Object);

            var (statusCode, body) = await InvokeAndReadAsync(middleware, context);

            Assert.Equal((int)HttpStatusCode.Forbidden, statusCode);
            Assert.Equal("Access denied.", body.GetProperty("message").GetString());
        }

        [Fact]
        public async Task UnhandledException_Returns500_AndGenericMessage()
        {
            var (context, logger) = CreateContext();
            RequestDelegate next = _ => throw new InvalidOperationException("DB connection lost.");
            var middleware = new ExceptionHandlingMiddleware(next, logger.Object);

            var (statusCode, body) = await InvokeAndReadAsync(middleware, context);

            Assert.Equal((int)HttpStatusCode.InternalServerError, statusCode);
            Assert.Equal("An unexpected error occurred. Please try again.", body.GetProperty("message").GetString());
            // Message must NOT leak internal exception details
            Assert.DoesNotContain("DB connection", body.GetProperty("message").GetString());
        }

        [Fact]
        public async Task NoException_PassesThrough_Untouched()
        {
            var (context, logger) = CreateContext();
            RequestDelegate next = ctx =>
            {
                ctx.Response.StatusCode = 200;
                return Task.CompletedTask;
            };
            var middleware = new ExceptionHandlingMiddleware(next, logger.Object);

            await middleware.InvokeAsync(context);

            Assert.Equal(200, context.Response.StatusCode);
        }
    }
}