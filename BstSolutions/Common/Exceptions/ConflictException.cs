namespace BstSolutions.Common;

public class ConflictException : Exception
{
    public string UserMessage { get; }

    public string ErrorCode { get; }

    public ConflictException(string userMessage, string errorCode)
        : base(userMessage)
    {
        UserMessage = userMessage;
        ErrorCode = errorCode;
    }
}
