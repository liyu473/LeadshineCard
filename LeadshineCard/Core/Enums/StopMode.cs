namespace LeadshineCard.Core.Enums;

/// <summary>
/// 停止模式枚举
/// </summary>
public enum StopMode : ushort
{
    /// <summary>
    /// 立即停止（急停）
    /// </summary>
    Immediate = 0,

    /// <summary>
    /// 减速停止
    /// </summary>
    Deceleration = 1,

    /// <summary>
    /// 紧急停止
    /// </summary>
    Emergency = 2,
}
