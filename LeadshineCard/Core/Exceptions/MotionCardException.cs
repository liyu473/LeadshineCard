namespace LeadshineCard.Core.Exceptions;

/// <summary>
/// 运动控制卡异常基类
/// </summary>
public class MotionCardException : Exception
{
    /// <summary>
    /// 错误码
    /// </summary>
    public short ErrorCode { get; }

    /// <summary>
    /// 构造函数
    /// </summary>
    public MotionCardException() : base()
    {
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="message">异常消息</param>
    public MotionCardException(string message) : base(message)
    {
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="message">异常消息</param>
    /// <param name="errorCode">错误码</param>
    public MotionCardException(string message, short errorCode) : base(message)
    {
        ErrorCode = errorCode;
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="message">异常消息</param>
    /// <param name="innerException">内部异常</param>
    public MotionCardException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="message">异常消息</param>
    /// <param name="errorCode">错误码</param>
    /// <param name="innerException">内部异常</param>
    public MotionCardException(string message, short errorCode, Exception innerException) 
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }
}
