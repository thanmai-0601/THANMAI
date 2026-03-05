using System.Net;
using System.Text.Json;

namespace API.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
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
                _logger.LogError(ex, "Unhandled exception occurred");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            context.Response.ContentType = "application/json";

            // Map exception types to HTTP status codes
            context.Response.StatusCode = ex switch
            {
                InvalidOperationException => (int)HttpStatusCode.BadRequest,       // 400
                UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,   // 401
                KeyNotFoundException => (int)HttpStatusCode.NotFound,              // 404
                _ => (int)HttpStatusCode.InternalServerError                       // 500
            };

            var response = new
            {
                StatusCode = context.Response.StatusCode,
                Message = ex.Message,
                // Only show stack trace in development
                Detail = context.RequestServices
                    .GetRequiredService<IWebHostEnvironment>()
                    .IsDevelopment() ? ex.StackTrace : null
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}
