namespace LeadshineCard.Core.Models;

/// <summary>
/// 插补速度参数
/// </summary>
public class InterpolationParameters
{
    /// <summary>
    /// 起始速度 (单位: mm/s)
    /// </summary>
    public double MinSpeed { get; set; } = 0.0;

    /// <summary>
    /// 最大速度 (单位: mm/s)
    /// </summary>
    public double MaxSpeed { get; set; } = 100.0;

    /// <summary>
    /// 加速时间 (单位: 秒)
    /// </summary>
    public double AccelerationTime { get; set; } = 0.5;

    /// <summary>
    /// 减速时间 (单位: 秒)
    /// </summary>
    public double DecelerationTime { get; set; } = 0.5;

    /// <summary>
    /// 停止速度 (单位: mm/s)
    /// </summary>
    public double StopSpeed { get; set; } = 0.0;

    /// <summary>
    /// 验证参数有效性
    /// </summary>
    public void Validate()
    {
        if (MaxSpeed <= 0)
            throw new ArgumentException("最大速度必须大于0", nameof(MaxSpeed));

        if (AccelerationTime <= 0)
            throw new ArgumentException("加速时间必须大于0", nameof(AccelerationTime));

        if (DecelerationTime <= 0)
            throw new ArgumentException("减速时间必须大于0", nameof(DecelerationTime));

        if (MinSpeed < 0)
            throw new ArgumentException("起始速度不能为负", nameof(MinSpeed));

        if (MinSpeed >= MaxSpeed)
            throw new ArgumentException("起始速度必须小于最大速度", nameof(MinSpeed));
    }
}

/// <summary>
/// 插补参数预设
/// </summary>
public static class InterpolationParametersPresets
{
    /// <summary>
    /// 高速模式 - 适用于快速轮廓加工
    /// </summary>
    public static InterpolationParameters HighSpeed => new()
    {
        MinSpeed = 1.0,
        MaxSpeed = 300.0,
        AccelerationTime = 0.3,
        DecelerationTime = 0.3,
        StopSpeed = 0.5
    };

    /// <summary>
    /// 精密模式 - 适用于精密轮廓加工
    /// </summary>
    public static InterpolationParameters Precision => new()
    {
        MinSpeed = 0.5,
        MaxSpeed = 80.0,
        AccelerationTime = 0.8,
        DecelerationTime = 0.8,
        StopSpeed = 0.2
    };

    /// <summary>
    /// 平衡模式 - 速度和精度的平衡
    /// </summary>
    public static InterpolationParameters Balanced => new()
    {
        MinSpeed = 1.0,
        MaxSpeed = 150.0,
        AccelerationTime = 0.5,
        DecelerationTime = 0.5,
        StopSpeed = 0.5
    };

    /// <summary>
    /// 雕刻模式 - 适用于雕刻、激光切割等
    /// </summary>
    public static InterpolationParameters Engraving => new()
    {
        MinSpeed = 0.2,
        MaxSpeed = 50.0,
        AccelerationTime = 1.0,
        DecelerationTime = 1.0,
        StopSpeed = 0.1
    };
}
