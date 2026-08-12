namespace BstSolutions.Common;

public class UnauthorizedException : Exception
{
    public string UserMessage { get; }

    public string ErrorCode { get; }

    public UnauthorizedException(string userMessage, string errorCode)
        : base(userMessage)
    {
        UserMessage = userMessage;
        ErrorCode = errorCode;
    }
}
