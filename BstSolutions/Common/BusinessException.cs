namespace BstSolutions.Common;

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
