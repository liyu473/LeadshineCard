namespace LeadshineCard.Core.Exceptions;

/// <summary>
/// 板卡初始化异常
/// </summary>
public class CardInitializationException : MotionCardException
{
    public CardInitializationException() : base()
    {
    }

    public CardInitializationException(string message) : base(message)
    {
    }

    public CardInitializationException(string message, short errorCode) 
        : base(message, errorCode)
    {
    }

    public CardInitializationException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }

    public CardInitializationException(string message, short errorCode, Exception innerException) 
        : base(message, errorCode, innerException)
    {
    }
}
