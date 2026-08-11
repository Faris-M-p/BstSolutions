using System.Diagnostics;

namespace BstSolutions.Middleware;

/// <summary>
/// Logs each HTTP request/response (method, path, status, duration, user).
/// Does not handle exceptions — GlobalExceptionMiddleware remains responsible.
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(
        RequestDelegate next,
        ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();

            var user = context.User?.Identity?.IsAuthenticated == true
                ? context.User.Identity.Name ?? "Anonymous"
                : "Anonymous";

            _logger.LogInformation(
                "HTTP {Method} {Path} responded {StatusCode} in {ElapsedMilliseconds} ms for user {User}, TraceId: {TraceId}",
                context.Request.Method,
                context.Request.Path.Value,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds,
                user,
                context.TraceIdentifier);
        }
    }
}
