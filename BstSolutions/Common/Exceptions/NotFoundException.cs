namespace BstSolutions.Common;

public class NotFoundException : Exception
{
    public string UserMessage { get; }

    public string ErrorCode { get; }

    public NotFoundException(string userMessage, string errorCode)
        : base(userMessage)
    {
        UserMessage = userMessage;
        ErrorCode = errorCode;
    }
}
