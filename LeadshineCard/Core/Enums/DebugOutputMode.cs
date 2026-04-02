namespace LeadshineCard.Core.Enums;

/// <summary>
/// 函数库调试输出模式。
/// </summary>
public enum DebugOutputMode : ushort
{
    /// <summary>
    /// 只打印报错函数。
    /// </summary>
    ErrorsOnly = 0,

    /// <summary>
    /// 打印全部函数调用。
    /// </summary>
    All = 1,

    /// <summary>
    /// 不打印任何函数调用。
    /// </summary>
    None = 2,
}
