namespace LeadshineCard.Core.Enums;

/// <summary>
/// 脉冲输出模式枚举
/// </summary>
public enum PulseOutputMode : ushort
{
    /// <summary>
    /// 脉冲+方向模式 (正方向: PULSE高电平, DIR低电平; 负方向: PULSE高电平, DIR高电平)
    /// </summary>
    PulseDirection0 = 0,

    /// <summary>
    /// 脉冲+方向模式 (正方向: PULSE高电平, DIR低电平; 负方向: PULSE低电平, DIR高电平)
    /// </summary>
    PulseDirection1 = 1,

    /// <summary>
    /// 脉冲+方向模式 (正方向: PULSE低电平, DIR高电平; 负方向: PULSE低电平, DIR低电平)
    /// </summary>
    PulseDirection2 = 2,

    /// <summary>
    /// 脉冲+方向模式 (正方向: PULSE低电平, DIR高电平; 负方向: PULSE高电平, DIR低电平)
    /// </summary>
    PulseDirection3 = 3,

    /// <summary>
    /// 双脉冲模式 (正方向: PULSE高电平; 负方向: DIR高电平)
    /// </summary>
    DoublePulse4 = 4,

    /// <summary>
    /// 双脉冲模式 (正方向: PULSE低电平; 负方向: DIR低电平)
    /// </summary>
    DoublePulse5 = 5
}
