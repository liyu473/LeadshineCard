using LeadshineCard.Core.Enums;
using LeadshineCard.Core.Models;

namespace LeadshineCard.Core.Interfaces;

/// <summary>
/// 轴控制器接口
/// </summary>
public interface IAxisController
{
    /// <summary>
    /// 轴号
    /// </summary>
    ushort AxisNo { get; }

    /// <summary>
    /// 设置运动参数
    /// </summary>
    /// <param name="parameters">运动参数</param>
    Task SetMotionParametersAsync(MotionParameters parameters);

    /// <summary>
    /// 获取运动参数
    /// </summary>
    /// <returns>运动参数</returns>
    Task<MotionParameters> GetMotionParametersAsync();

    /// <summary>
    /// 相对运动
    /// </summary>
    /// <param name="distance">运动距离 (单位: mm 或 度)</param>
    /// <returns>是否成功</returns>
    Task<bool> MoveRelativeAsync(double distance);

    /// <summary>
    /// 绝对运动
    /// </summary>
    /// <param name="position">目标位置 (单位: mm 或 度)</param>
    /// <returns>是否成功</returns>
    Task<bool> MoveAbsoluteAsync(double position);

    /// <summary>
    /// JOG运动
    /// </summary>
    /// <param name="positiveDirection">是否为正方向</param>
    /// <returns>是否成功</returns>
    Task<bool> JogAsync(bool positiveDirection);

    /// <summary>
    /// 停止运动
    /// </summary>
    /// <param name="mode">停止模式</param>
    /// <returns>是否成功</returns>
    Task<bool> StopAsync(StopMode mode);

    /// <summary>
    /// 获取当前位置
    /// </summary>
    /// <returns>当前位置 (单位: mm 或 度)</returns>
    Task<double> GetCurrentPositionAsync();

    /// <summary>
    /// 设置当前位置
    /// </summary>
    /// <param name="position">位置值 (单位: mm 或 度)</param>
    /// <returns>是否成功</returns>
    Task<bool> SetCurrentPositionAsync(double position);

    /// <summary>
    /// 获取当前速度
    /// </summary>
    /// <returns>当前速度 (单位: mm/s 或 度/s)</returns>
    Task<double> GetCurrentSpeedAsync();

    /// <summary>
    /// 获取目标位置
    /// </summary>
    /// <returns>目标位置 (单位: mm 或 度)</returns>
    Task<double> GetTargetPositionAsync();

    /// <summary>
    /// 获取轴状态
    /// </summary>
    /// <returns>轴状态</returns>
    Task<AxisStatus> GetStatusAsync();

    /// <summary>
    /// 检查运动是否完成
    /// </summary>
    /// <returns>是否完成</returns>
    Task<bool> CheckDoneAsync();

    /// <summary>
    /// 在线变速
    /// </summary>
    /// <param name="newSpeed">新速度 (单位: mm/s 或 度/s)</param>
    /// <param name="accelTime">加速时间 (单位: 秒)</param>
    /// <returns>是否成功</returns>
    Task<bool> ChangeSpeedAsync(double newSpeed, double accelTime);
}
