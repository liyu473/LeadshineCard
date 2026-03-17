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

    /// <summary>
    /// 设置回零模式
    /// </summary>
    /// <param name="direction">回零方向</param>
    /// <param name="speed">回零速度 (单位: mm/s 或 度/s)</param>
    /// <param name="mode">回零模式</param>
    /// <returns>是否成功</returns>
    Task<bool> SetHomeModeAsync(HomeDirection direction, double speed, HomeMode mode);

    /// <summary>
    /// 执行回零运动
    /// </summary>
    /// <returns>是否成功</returns>
    Task<bool> HomeMoveAsync();

    /// <summary>
    /// 获取回零结果
    /// </summary>
    /// <returns>回零状态 (0=未完成, 1=成功, 2=失败)</returns>
    Task<ushort> GetHomeResultAsync();

    /// <summary>
    /// 设置回零后的位置
    /// </summary>
    /// <param name="position">回零后设置的位置值 (单位: mm 或 度)</param>
    /// <returns>是否成功</returns>
    Task<bool> SetHomePositionAsync(double position);

    /// <summary>
    /// 等待回零完成
    /// </summary>
    /// <param name="timeoutMs">超时时间(毫秒)，0表示无限等待</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>是否成功完成回零</returns>
    Task<bool> WaitHomeCompleteAsync(int timeoutMs = 30000, CancellationToken cancellationToken = default);

    /// <summary>
    /// 设置 PVT 表 (Position-Velocity-Time)
    /// </summary>
    /// <param name="times">时间数组 (单位: 秒)</param>
    /// <param name="positions">位置数组 (单位: mm 或 度)</param>
    /// <param name="velocities">速度数组 (单位: mm/s 或 度/s)</param>
    /// <returns>是否成功</returns>
    Task<bool> SetPvtTableAsync(double[] times, double[] positions, double[] velocities);

    /// <summary>
    /// 设置 PVTS 表 (Position-Velocity(start/end)-Time-Smooth)
    /// </summary>
    /// <param name="times">时间数组 (单位: 秒)</param>
    /// <param name="positions">位置数组 (单位: mm 或 度)</param>
    /// <param name="startVelocity">起始速度 (单位: mm/s 或 度/s)</param>
    /// <param name="endVelocity">结束速度 (单位: mm/s 或 度/s)</param>
    /// <returns>是否成功</returns>
    Task<bool> SetPvtsTableAsync(
        double[] times,
        double[] positions,
        double startVelocity,
        double endVelocity
    );

    /// <summary>
    /// 设置 PTS 表 (Position-Time-Smooth with percent)
    /// </summary>
    /// <param name="times">时间数组 (单位: 秒)</param>
    /// <param name="positions">位置数组 (单位: mm 或 度)</param>
    /// <param name="percents">百分比数组 (0-1之间)</param>
    /// <returns>是否成功</returns>
    Task<bool> SetPtsTableAsync(double[] times, double[] positions, double[] percents);

    /// <summary>
    /// 设置 PTT 表 (Position-Time-Time)
    /// </summary>
    /// <param name="times">时间数组 (单位: 秒)</param>
    /// <param name="positions">位置数组 (单位: mm 或 度)</param>
    /// <returns>是否成功</returns>
    Task<bool> SetPttTableAsync(double[] times, double[] positions);

    /// <summary>
    /// 开始 PVT 运动
    /// </summary>
    /// <returns>是否成功</returns>
    Task<bool> StartPvtMoveAsync();

    /// <summary>
    /// 获取 PVT 缓冲区剩余空间
    /// </summary>
    /// <returns>剩余空间大小</returns>
    Task<short> GetPvtRemainSpaceAsync();

    /// <summary>
    /// 等待运动完成
    /// </summary>
    /// <param name="timeoutMs">超时时间(毫秒)，0表示无限等待</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>是否成功完成运动</returns>
    Task<bool> WaitMotionCompleteAsync(int timeoutMs = 30000, CancellationToken cancellationToken = default);

    /// <summary>
    /// 设置软限位
    /// </summary>
    /// <param name="softLimit">软限位配置</param>
    void SetSoftLimit(SoftLimit softLimit);

    /// <summary>
    /// 获取软限位
    /// </summary>
    /// <returns>软限位配置</returns>
    SoftLimit? GetSoftLimit();
}
