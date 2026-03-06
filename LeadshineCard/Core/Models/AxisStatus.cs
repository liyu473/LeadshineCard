using LeadshineCard.Core.Enums;

namespace LeadshineCard.Core.Models;

/// <summary>
/// 轴状态信息
/// </summary>
public class AxisStatus
{
    /// <summary>
    /// 轴号
    /// </summary>
    public ushort AxisNo { get; set; }

    /// <summary>
    /// 轴状态
    /// </summary>
    public AxisState State { get; set; }

    /// <summary>
    /// 当前位置 (单位: mm 或 度)
    /// </summary>
    public double Position { get; set; }

    /// <summary>
    /// 当前速度 (单位: mm/s 或 度/s)
    /// </summary>
    public double Speed { get; set; }

    /// <summary>
    /// 目标位置 (单位: mm 或 度)
    /// </summary>
    public double TargetPosition { get; set; }

    /// <summary>
    /// 编码器位置 (单位: mm 或 度)
    /// </summary>
    public double EncoderPosition { get; set; }

    /// <summary>
    /// 是否触发正限位
    /// </summary>
    public bool PositiveLimit { get; set; }

    /// <summary>
    /// 是否触发负限位
    /// </summary>
    public bool NegativeLimit { get; set; }

    /// <summary>
    /// 是否在原点
    /// </summary>
    public bool Home { get; set; }

    /// <summary>
    /// 是否报警
    /// </summary>
    public bool Alarm { get; set; }

    /// <summary>
    /// 是否使能
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// 是否运动完成
    /// </summary>
    public bool Done { get; set; }

    /// <summary>
    /// 错误码
    /// </summary>
    public short ErrorCode { get; set; }

    /// <summary>
    /// 时间戳
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.Now;

    /// <summary>
    /// 转换为字符串
    /// </summary>
    public override string ToString()
    {
        return $"Axis{AxisNo}: State={State}, Pos={Position:F3}, Speed={Speed:F2}, " +
               $"PLimit={PositiveLimit}, NLimit={NegativeLimit}, Home={Home}, Alarm={Alarm}";
    }
}
