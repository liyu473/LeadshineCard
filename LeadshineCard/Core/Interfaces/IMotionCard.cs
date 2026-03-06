using LeadshineCard.Core.Models;

namespace LeadshineCard.Core.Interfaces;

/// <summary>
/// 运动控制卡接口
/// </summary>
public interface IMotionCard : IDisposable
{
    /// <summary>
    /// 板卡号
    /// </summary>
    ushort CardNo { get; }

    /// <summary>
    /// 是否已连接
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// 初始化板卡
    /// </summary>
    /// <param name="cardNo">板卡号</param>
    /// <returns>是否成功</returns>
    Task<bool> InitializeAsync(ushort cardNo);

    /// <summary>
    /// 关闭板卡
    /// </summary>
    /// <returns>是否成功</returns>
    Task<bool> CloseAsync();

    /// <summary>
    /// 复位板卡
    /// </summary>
    /// <returns>是否成功</returns>
    Task<bool> ResetAsync();

    /// <summary>
    /// 获取板卡信息
    /// </summary>
    /// <returns>板卡信息</returns>
    CardInfo GetCardInfo();

    /// <summary>
    /// 获取轴控制器
    /// </summary>
    /// <param name="axisNo">轴号</param>
    /// <returns>轴控制器</returns>
    IAxisController GetAxisController(ushort axisNo);

    /// <summary>
    /// 获取IO控制器
    /// </summary>
    /// <returns>IO控制器</returns>
    IIoController GetIoController();

    /// <summary>
    /// 获取插补控制器
    /// </summary>
    /// <returns>插补控制器</returns>
    IInterpolationController GetInterpolationController();

    /// <summary>
    /// 紧急停止所有轴
    /// </summary>
    /// <returns>是否成功</returns>
    Task<bool> EmergencyStopAsync();
}
