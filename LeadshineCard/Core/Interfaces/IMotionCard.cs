using LeadshineCard.Core.Enums;
using LeadshineCard.Core.Models;

namespace LeadshineCard.Core.Interfaces;

/// <summary>
/// 运动控制卡接口。
/// </summary>
public interface IMotionCard : IDisposable
{
    /// <summary>
    /// 板卡号。
    /// </summary>
    ushort CardNo { get; }

    /// <summary>
    /// 是否已连接。
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// 初始化板卡。
    /// </summary>
    /// <returns>是否成功</returns>
    Task<bool> InitializeAsync();

    /// <summary>
    /// 关闭板卡。
    /// </summary>
    /// <returns>是否成功</returns>
    Task<bool> CloseAsync();

    /// <summary>
    /// 复位板卡。
    /// </summary>
    /// <returns>是否成功</returns>
    Task<bool> ResetAsync();

    /// <summary>
    /// 设置函数库调试输出模式。
    /// 这是全局库设置，不是单张板卡私有设置。
    /// </summary>
    /// <param name="mode">调试输出模式</param>
    /// <param name="fileName">日志文件路径</param>
    /// <returns>是否成功</returns>
    Task<bool> SetDebugModeAsync(DebugOutputMode mode, string fileName);

    /// <summary>
    /// 获取函数库调试输出设置。
    /// 这是全局库设置，不是单张板卡私有设置。
    /// </summary>
    /// <returns>调试输出设置</returns>
    Task<DebugModeSettings> GetDebugModeAsync();

    /// <summary>
    /// 获取板卡信息。
    /// </summary>
    /// <returns>板卡信息</returns>
    CardInfo GetCardInfo();

    /// <summary>
    /// 获取轴控制器。
    /// </summary>
    /// <param name="axisNo">轴号</param>
    /// <returns>轴控制器</returns>
    IAxisController GetAxisController(ushort axisNo);

    /// <summary>
    /// 获取 IO 控制器。
    /// </summary>
    /// <returns>IO 控制器</returns>
    IIoController GetIoController();

    /// <summary>
    /// 获取插补控制器。
    /// </summary>
    /// <returns>插补控制器</returns>
    IInterpolationController GetInterpolationController();

    /// <summary>
    /// 紧急停止所有轴。
    /// </summary>
    /// <returns>是否成功</returns>
    Task<bool> EmergencyStopAsync();
}
