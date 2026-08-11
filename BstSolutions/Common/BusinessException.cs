namespace BstSolutions.Common;

/// <summary>
/// Expected business-rule failure.
/// Controllers map UserMessage to ModelState/ApiResponse.
/// Middleware maps unexpected exceptions only (not this type when caught in controllers).
/// </summary>
public class BusinessException : Exception
{
    public string UserMessage { get; }

    public string DeveloperMessage { get; }

    public string ErrorCode { get; }

    public BusinessException(string userMessage, string developerMessage, string errorCode)
        : base(userMessage)
    {
        UserMessage = userMessage;
        DeveloperMessage = developerMessage;
        ErrorCode = errorCode;
    }
}
