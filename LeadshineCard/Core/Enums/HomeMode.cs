namespace LeadshineCard.Core.Enums;

/// <summary>
/// 回零模式枚举
/// </summary>
public enum HomeMode
{
    /// <summary>
    /// 使用原点开关回零
    /// </summary>
    HomeSwitch = 0,

    /// <summary>
    /// 使用编码器Z相回零
    /// </summary>
    EncoderIndex = 1,

    /// <summary>
    /// 使用限位开关回零
    /// </summary>
    LimitSwitch = 2,

    /// <summary>
    /// 原点开关+Z相
    /// </summary>
    HomeSwitchAndIndex = 3
}
