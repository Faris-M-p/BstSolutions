namespace BstSolutions.Common;

/// <summary>
/// Simple business-rule failure. Controllers convert this into user-friendly messages.
/// </summary>
public class BusinessException : Exception
{
    public BusinessException(string message) : base(message)
    {
    }
}
