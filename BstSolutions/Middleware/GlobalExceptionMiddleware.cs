using System.Net;
using BstSolutions.Common;
using BstSolutions.Common.Responses;

namespace BstSolutions.Middleware;

/// <summary>
/// Catches unhandled exceptions, logs technical details, returns safe user messages only.
/// </summary>
public class GlobalExceptionMiddleware
{
    private const string InternalErrorCode = "INTERNAL_SERVER_ERROR";
    private const string SafeUserMessage = "Something went wrong. Please try again later.";

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
        if (context.Response.HasStarted)
        {
            throw exception;
        }

        var reference = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();

        if (exception is BusinessException businessException)
        {
            _logger.LogWarning(
                businessException,
                "Business exception. ErrorCode: {ErrorCode}. DeveloperMessage: {DeveloperMessage}. Reference: {Reference}",
                businessException.ErrorCode,
                businessException.DeveloperMessage,
                reference);

            await WriteResponseAsync(
                context,
                HttpStatusCode.BadRequest,
                ApiResponse.Fail(businessException.UserMessage, businessException.ErrorCode));
            return;
        }

        _logger.LogError(
            exception,
            "Unhandled exception. ErrorCode: {ErrorCode}. DeveloperMessage: Unexpected exception for {Method} {Path}. Reference: {Reference}",
            InternalErrorCode,
            context.Request.Method,
            context.Request.Path,
            reference);

        var userMessage = $"{SafeUserMessage} Reference: {reference}.";

        await WriteResponseAsync(
            context,
            HttpStatusCode.InternalServerError,
            ApiResponse.Fail(userMessage, InternalErrorCode));
    }

    private static async Task WriteResponseAsync(HttpContext context, HttpStatusCode statusCode, ApiResponse response)
    {
        // Never include DeveloperMessage in HTTP payloads for unexpected failures.
        response.DeveloperMessage = null;

        context.Response.Clear();
        context.Response.StatusCode = (int)statusCode;

        var acceptsJson = context.Request.Headers.Accept.Any(v =>
            v != null && v.Contains("application/json", StringComparison.OrdinalIgnoreCase));

        var isAjax = string.Equals(
            context.Request.Headers["X-Requested-With"],
            "XMLHttpRequest",
            StringComparison.OrdinalIgnoreCase);

        if (acceptsJson || isAjax || IsApiPath(context.Request.Path))
        {
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(response);
            return;
        }

        context.Items["ErrorUserMessage"] = response.UserMessage;
        context.Response.Redirect($"/Home/Error?ref={Uri.EscapeDataString(response.ErrorCode ?? InternalErrorCode)}");
    }

    private static bool IsApiPath(PathString path) =>
        path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase);
}
