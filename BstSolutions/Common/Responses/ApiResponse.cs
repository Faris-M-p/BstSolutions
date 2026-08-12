namespace BstSolutions.Common.Responses;

public class ApiResponse
{
    public bool Success { get; set; }

    public string UserMessage { get; set; } = string.Empty;

    public string? ErrorCode { get; set; }

    public static ApiResponse Ok(string userMessage) => new()
    {
        Success = true,
        UserMessage = userMessage
    };

    public static ApiResponse Fail(string userMessage, string? errorCode = null) => new()
    {
        Success = false,
        UserMessage = userMessage,
        ErrorCode = errorCode
    };
}

public class ApiResponse<T> : ApiResponse
{
    public T? Data { get; set; }

    public static ApiResponse<T> Ok(T data, string userMessage = "") => new()
    {
        Success = true,
        UserMessage = userMessage,
        Data = data
    };

    public new static ApiResponse<T> Fail(string userMessage, string? errorCode = null) => new()
    {
        Success = false,
        UserMessage = userMessage,
        ErrorCode = errorCode
    };
}
