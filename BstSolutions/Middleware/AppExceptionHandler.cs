using System.Text.Json;
using BstSolutions.Common;
using BstSolutions.Common.Responses;
using Microsoft.AspNetCore.Diagnostics;

namespace BstSolutions.Middleware;

public class AppExceptionHandler : IExceptionHandler
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly ILogger<AppExceptionHandler> _logger;

    public AppExceptionHandler(ILogger<AppExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext context,Exception exception,CancellationToken cancellationToken)
    {
        int statusCode;
        string userMessage;
        string? errorCode;

        if (exception is UnauthorizedException unauthorizedException)
        {
            statusCode = StatusCodes.Status401Unauthorized;
            userMessage = unauthorizedException.UserMessage;
            errorCode = unauthorizedException.ErrorCode;

            _logger.LogWarning(
                unauthorizedException,
                "Unauthorized. ErrorCode: {ErrorCode}. Path: {Path}",
                errorCode,
                context.Request.Path);
        }
        else if (exception is NotFoundException notFoundException)
        {
            statusCode = StatusCodes.Status404NotFound;
            userMessage = notFoundException.UserMessage;
            errorCode = notFoundException.ErrorCode;

            _logger.LogWarning(
                notFoundException,
                "Not found. ErrorCode: {ErrorCode}. Path: {Path}",
                errorCode,
                context.Request.Path);
        }
        else if (exception is ConflictException conflictException)
        {
            statusCode = StatusCodes.Status409Conflict;
            userMessage = conflictException.UserMessage;
            errorCode = conflictException.ErrorCode;

            _logger.LogWarning(
                conflictException,
                "Conflict. ErrorCode: {ErrorCode}. Path: {Path}",
                errorCode,
                context.Request.Path);
        }
        else if (exception is BusinessException businessException)
        {
            statusCode = StatusCodes.Status400BadRequest;
            userMessage = businessException.UserMessage;
            errorCode = businessException.ErrorCode;

            _logger.LogWarning(
                businessException,
                "Business exception. ErrorCode: {ErrorCode}. Path: {Path}",
                errorCode,
                context.Request.Path);
        }
        else
        {
            statusCode = StatusCodes.Status500InternalServerError;
            userMessage = "Something went wrong. Please try again later.";
            errorCode = null;

            _logger.LogError(
                exception,
                "Unhandled exception for {Method} {Path}",
                context.Request.Method,
                context.Request.Path);
        }

        context.Response.StatusCode = statusCode;

        var isAjax = string.Equals(context.Request.Headers["X-Requested-With"],"XMLHttpRequest",StringComparison.OrdinalIgnoreCase);

        if (isAjax)
        {
            context.Response.ContentType = "application/json";
            var payload = ApiResponse.Fail(userMessage, errorCode);
            await context.Response.WriteAsync(JsonSerializer.Serialize(payload, JsonOptions),cancellationToken);
            return true;
        }

        context.Response.Redirect("/Account/Error");
        return true;
    }
}
