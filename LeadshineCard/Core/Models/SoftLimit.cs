namespace LeadshineCard.Core.Models;

/// <summary>
/// 软限位配置
/// </summary>
public class SoftLimit
{
    /// <summary>
    /// 是否启用软限位
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// 正向软限位位置
    /// </summary>
    public double PositiveLimit { get; set; }

    /// <summary>
    /// 负向软限位位置
    /// </summary>
    public double NegativeLimit { get; set; }

    /// <summary>
    /// 验证位置是否在软限位范围内
    /// </summary>
    public bool IsWithinLimits(double position)
    {
        if (!Enabled)
            return true;

        return position >= NegativeLimit && position <= PositiveLimit;
    }

    /// <summary>
    /// 获取限位后的安全位置
    /// </summary>
    public double ClampPosition(double position)
    {
        if (!Enabled)
            return position;

        if (position > PositiveLimit)
            return PositiveLimit;
        if (position < NegativeLimit)
            return NegativeLimit;

        return position;
    }
}
