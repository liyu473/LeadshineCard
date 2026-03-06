namespace LeadshineCard.Core.Models;

/// <summary>
/// 板卡信息
/// </summary>
public class CardInfo
{
    /// <summary>
    /// 板卡号
    /// </summary>
    public ushort CardNo { get; set; }

    /// <summary>
    /// 板卡类型
    /// </summary>
    public uint CardType { get; set; }

    /// <summary>
    /// 板卡ID
    /// </summary>
    public ushort CardId { get; set; }

    /// <summary>
    /// 板卡版本
    /// </summary>
    public uint CardVersion { get; set; }

    /// <summary>
    /// 固件版本
    /// </summary>
    public uint FirmwareVersion { get; set; }

    /// <summary>
    /// 子固件版本
    /// </summary>
    public uint SubFirmwareVersion { get; set; }

    /// <summary>
    /// 库版本
    /// </summary>
    public uint LibraryVersion { get; set; }

    /// <summary>
    /// 总轴数
    /// </summary>
    public uint TotalAxes { get; set; }

    /// <summary>
    /// 总输入IO数
    /// </summary>
    public ushort TotalInputs { get; set; }

    /// <summary>
    /// 总输出IO数
    /// </summary>
    public ushort TotalOutputs { get; set; }

    /// <summary>
    /// 是否已连接
    /// </summary>
    public bool IsConnected { get; set; }

    /// <summary>
    /// 初始化时间
    /// </summary>
    public DateTime InitializedTime { get; set; }

    /// <summary>
    /// 转换为字符串
    /// </summary>
    public override string ToString()
    {
        return $"Card{CardNo}: Type=0x{CardType:X}, Version={CardVersion}, " +
               $"Axes={TotalAxes}, IO={TotalInputs}/{TotalOutputs}, Connected={IsConnected}";
    }
}
