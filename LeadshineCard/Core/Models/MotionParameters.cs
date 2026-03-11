namespace LeadshineCard.Core.Models;

/// <summary>
/// 运动参数
/// </summary>
public class MotionParameters
{
    /// <summary>
    /// 起始速度 (单位: mm/s 或 度/s)
    /// </summary>
    public double MinSpeed { get; set; } = 0.0;

    /// <summary>
    /// 最大速度 (单位: mm/s 或 度/s)
    /// </summary>
    public double MaxSpeed { get; set; } = 100.0;

    /// <summary>
    /// 加速度 (单位: mm/s² 或 度/s²)
    /// </summary>
    public double Acceleration { get; set; } = 500.0;

    /// <summary>
    /// 减速度 (单位: mm/s² 或 度/s²)
    /// </summary>
    public double Deceleration { get; set; } = 500.0;

    /// <summary>
    /// 停止速度 (单位: mm/s 或 度/s)
    /// </summary>
    public double StopSpeed { get; set; } = 0.0;

    /// <summary>
    /// S曲线时间 (单位: 秒)
    /// </summary>
    public double SCurveTime { get; set; } = 0.0;

    /// <summary>
    /// 脉冲当量 (单位: mm/脉冲 或 度/脉冲)
    /// </summary>
    public double PulseEquivalent { get; set; } = 0.001;

    /// <summary>
    /// 验证参数有效性
    /// </summary>
    public void Validate()
    {
        if (MaxSpeed <= 0)
            throw new ArgumentException("最大速度必须大于0", nameof(MaxSpeed));

        if (Acceleration <= 0)
            throw new ArgumentException("加速度必须大于0", nameof(Acceleration));

        if (Deceleration <= 0)
            throw new ArgumentException("减速度必须大于0", nameof(Deceleration));

        if (MinSpeed < 0)
            throw new ArgumentException("起始速度不能为负", nameof(MinSpeed));

        if (MinSpeed >= MaxSpeed)
            throw new ArgumentException("起始速度必须小于最大速度", nameof(MinSpeed));

        if (PulseEquivalent <= 0)
            throw new ArgumentException("脉冲当量必须大于0", nameof(PulseEquivalent));
    }
}
