namespace BstSolutions.Common.Results;

public class ServiceResult
{
    public bool Success { get; init; }

    public string UserMessage { get; init; } = string.Empty;

    public string? DeveloperMessage { get; init; }

    public string? ErrorCode { get; init; }

    public static ServiceResult Ok(string userMessage = "") => new()
    {
        Success = true,
        UserMessage = userMessage
    };

    public static ServiceResult Fail(string userMessage, string developerMessage, string errorCode) => new()
    {
        Success = false,
        UserMessage = userMessage,
        DeveloperMessage = developerMessage,
        ErrorCode = errorCode
    };
}
