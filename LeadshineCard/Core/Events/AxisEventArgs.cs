using LeadshineCard.Core.Models;

namespace LeadshineCard.Core.Events;

/// <summary>
/// 轴事件参数基类
/// </summary>
public class AxisEventArgs : EventArgs
{
    public ushort AxisNo { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.Now;
}

/// <summary>
/// 运动完成事件参数
/// </summary>
public class MotionCompletedEventArgs : AxisEventArgs
{
    public double FinalPosition { get; set; }
    public bool Success { get; set; }
}

/// <summary>
/// 限位触发事件参数
/// </summary>
public class LimitTriggeredEventArgs : AxisEventArgs
{
    public bool IsPositiveLimit { get; set; }
    public double Position { get; set; }
}

/// <summary>
/// 报警事件参数
/// </summary>
public class AlarmEventArgs : AxisEventArgs
{
    public string Message { get; set; } = string.Empty;
    public short ErrorCode { get; set; }
}

/// <summary>
/// 回零完成事件参数
/// </summary>
public class HomeCompletedEventArgs : AxisEventArgs
{
    public bool Success { get; set; }
    public double HomePosition { get; set; }
}

/// <summary>
/// 状态变化事件参数
/// </summary>
public class StatusChangedEventArgs : AxisEventArgs
{
    public AxisStatus Status { get; set; } = new();
}
