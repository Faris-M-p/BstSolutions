namespace BstSolutions.Common;

/// <summary>
/// Expected business-rule failure. Prefer returning ServiceResult from services.
/// If thrown, middleware maps it to a safe 400 response (not 500).
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
