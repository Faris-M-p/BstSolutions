using System.Net;

namespace BstSolutions.Middleware;

/// <summary>
/// Global exception handling middleware for cross-cutting HTTP error concerns.
/// Does not contain business logic.
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger)
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
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        // Log full exception details server-side only.
        _logger.LogError(exception, "Unhandled exception for {Method} {Path}", context.Request.Method, context.Request.Path);

        // Never expose stack traces, SQL, connection strings, or internal details to the client.
        const string userMessage = "An unexpected error occurred. Please try again later.";

        if (context.Response.HasStarted)
        {
            throw exception;
        }

        context.Response.Clear();
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        var acceptsJson = context.Request.Headers.Accept.Any(v =>
            v != null && v.Contains("application/json", StringComparison.OrdinalIgnoreCase));

        var isAjax = string.Equals(
            context.Request.Headers["X-Requested-With"],
            "XMLHttpRequest",
            StringComparison.OrdinalIgnoreCase);

        if (acceptsJson || isAjax)
        {
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new
            {
                success = false,
                message = userMessage
            });
            return;
        }

        // MVC-friendly redirect to a safe error page. Do not append exception details.
        context.Response.Redirect("/Home/Error");
    }
}
