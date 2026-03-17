using LeadshineCard.Core.Models;

namespace LeadshineCard.Core.Events;

/// <summary>
/// 插补事件参数基类
/// </summary>
public class InterpolationEventArgs : EventArgs
{
    public ushort CoordinateSystem { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.Now;
}

/// <summary>
/// 插补完成事件参数
/// </summary>
public class InterpolationCompletedEventArgs : InterpolationEventArgs
{
    public bool Success { get; set; }
    public int TotalSegments { get; set; }
}

/// <summary>
/// 插补段完成事件参数
/// </summary>
public class SegmentCompletedEventArgs : InterpolationEventArgs
{
    public int SegmentMark { get; set; }
}

/// <summary>
/// 缓冲区状态事件参数
/// </summary>
public class BufferStatusEventArgs : InterpolationEventArgs
{
    public int RemainingSpace { get; set; }
    public bool IsLow { get; set; }
}
