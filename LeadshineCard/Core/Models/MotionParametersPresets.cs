namespace LeadshineCard.Core.Models;

/// <summary>
/// 运动参数预设
/// </summary>
public static class MotionParametersPresets
{
    /// <summary>
    /// 高速模式 - 适用于快速定位，精度要求不高的场景
    /// </summary>
    public static MotionParameters HighSpeed => new()
    {
        PulseEquivalent = 0.001,
        MinSpeed = 1.0,
        MaxSpeed = 200.0,
        Acceleration = 1000.0,
        Deceleration = 1000.0,
        StopSpeed = 0.5,
        SCurveTime = 0.02
    };

    /// <summary>
    /// 精密模式 - 适用于高精度定位场景
    /// </summary>
    public static MotionParameters Precision => new()
    {
        PulseEquivalent = 0.001,
        MinSpeed = 0.1,
        MaxSpeed = 50.0,
        Acceleration = 200.0,
        Deceleration = 200.0,
        StopSpeed = 0.1,
        SCurveTime = 0.1
    };

    /// <summary>
    /// 重载模式 - 适用于大负载、需要平稳启停的场景
    /// </summary>
    public static MotionParameters HeavyLoad => new()
    {
        PulseEquivalent = 0.001,
        MinSpeed = 0.5,
        MaxSpeed = 80.0,
        Acceleration = 300.0,
        Deceleration = 300.0,
        StopSpeed = 0.2,
        SCurveTime = 0.15
    };

    /// <summary>
    /// 平衡模式 - 速度和精度的平衡，适用于大多数场景
    /// </summary>
    public static MotionParameters Balanced => new()
    {
        PulseEquivalent = 0.001,
        MinSpeed = 0.5,
        MaxSpeed = 100.0,
        Acceleration = 500.0,
        Deceleration = 500.0,
        StopSpeed = 0.2,
        SCurveTime = 0.05
    };

    /// <summary>
    /// 调试模式 - 低速运行，便于观察和调试
    /// </summary>
    public static MotionParameters Debug => new()
    {
        PulseEquivalent = 0.001,
        MinSpeed = 0.1,
        MaxSpeed = 10.0,
        Acceleration = 50.0,
        Deceleration = 50.0,
        StopSpeed = 0.1,
        SCurveTime = 0.05
    };
}
