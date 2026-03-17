namespace LeadshineCard.Core.Enums;

/// <summary>
/// 回零结果状态
/// </summary>
public enum HomeResultState : ushort
{
    /// <summary>
    /// 回零进行中
    /// </summary>
    InProgress = 0,

    /// <summary>
    /// 回零成功
    /// </summary>
    Success = 1,

    /// <summary>
    /// 回零失败
    /// </summary>
    Failed = 2
}
