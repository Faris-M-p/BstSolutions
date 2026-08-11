namespace BstSolutions.Common.Results;

public class ServiceResult<T> : ServiceResult
{
    public T? Data { get; init; }

    public static ServiceResult<T> Ok(T data, string userMessage = "") => new()
    {
        Success = true,
        UserMessage = userMessage,
        Data = data
    };

    public new static ServiceResult<T> Fail(string userMessage, string developerMessage, string errorCode) => new()
    {
        Success = false,
        UserMessage = userMessage,
        DeveloperMessage = developerMessage,
        ErrorCode = errorCode
    };
}
