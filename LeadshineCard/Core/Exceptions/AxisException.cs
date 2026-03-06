namespace LeadshineCard.Core.Exceptions;

/// <summary>
/// 轴异常基类
/// </summary>
public class AxisException : MotionCardException
{
    /// <summary>
    /// 轴号
    /// </summary>
    public ushort AxisNo { get; }

    public AxisException() : base()
    {
    }

    public AxisException(string message) : base(message)
    {
    }

    public AxisException(string message, ushort axisNo) : base(message)
    {
        AxisNo = axisNo;
    }

    public AxisException(string message, short errorCode) : base(message, errorCode)
    {
    }

    public AxisException(string message, ushort axisNo, short errorCode) 
        : base(message, errorCode)
    {
        AxisNo = axisNo;
    }

    public AxisException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }

    public AxisException(string message, ushort axisNo, Exception innerException) 
        : base(message, innerException)
    {
        AxisNo = axisNo;
    }
}

/// <summary>
/// 轴运动异常
/// </summary>
public class AxisMotionException : AxisException
{
    public AxisMotionException() : base()
    {
    }

    public AxisMotionException(string message) : base(message)
    {
    }

    public AxisMotionException(string message, ushort axisNo) : base(message, axisNo)
    {
    }

    public AxisMotionException(string message, ushort axisNo, short errorCode) 
        : base(message, axisNo, errorCode)
    {
    }
}

/// <summary>
/// 轴限位异常
/// </summary>
public class AxisLimitException : AxisException
{
    /// <summary>
    /// 是否为正限位
    /// </summary>
    public bool IsPositiveLimit { get; }

    public AxisLimitException() : base()
    {
    }

    public AxisLimitException(string message, ushort axisNo, bool isPositiveLimit) 
        : base(message, axisNo)
    {
        IsPositiveLimit = isPositiveLimit;
    }
}

/// <summary>
/// 轴回零异常
/// </summary>
public class AxisHomeException : AxisException
{
    public AxisHomeException() : base()
    {
    }

    public AxisHomeException(string message) : base(message)
    {
    }

    public AxisHomeException(string message, ushort axisNo) : base(message, axisNo)
    {
    }

    public AxisHomeException(string message, ushort axisNo, short errorCode) 
        : base(message, axisNo, errorCode)
    {
    }
}
