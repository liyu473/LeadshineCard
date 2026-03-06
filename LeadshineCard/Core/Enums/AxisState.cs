namespace LeadshineCard.Core.Enums;

/// <summary>
/// 轴状态枚举
/// </summary>
public enum AxisState
{
    /// <summary>
    /// 空闲状态
    /// </summary>
    Idle = 0,

    /// <summary>
    /// 运动中
    /// </summary>
    Moving = 1,

    /// <summary>
    /// 停止中
    /// </summary>
    Stopping = 2,

    /// <summary>
    /// 错误状态
    /// </summary>
    Error = 3,

    /// <summary>
    /// 回零中
    /// </summary>
    Homing = 4
}
