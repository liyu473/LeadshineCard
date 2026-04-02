using LeadshineCard.Core.Enums;

namespace LeadshineCard.Core.Models;

/// <summary>
/// 函数库调试输出设置。
/// </summary>
public class DebugModeSettings
{
    /// <summary>
    /// 调试输出模式。
    /// </summary>
    public DebugOutputMode Mode { get; set; }

    /// <summary>
    /// 调试日志文件路径。
    /// </summary>
    public string FileName { get; set; } = string.Empty;
}
