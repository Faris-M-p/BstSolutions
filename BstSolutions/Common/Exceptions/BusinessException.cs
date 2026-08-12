namespace BstSolutions.Common;

public class BusinessException : Exception
{
    public string UserMessage { get; }

    public string ErrorCode { get; }

    public BusinessException(string userMessage, string errorCode)
        : base(userMessage)
    {
        UserMessage = userMessage;
        ErrorCode = errorCode;
    }
}
