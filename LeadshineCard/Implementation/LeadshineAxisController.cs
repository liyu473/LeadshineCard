using LeadshineCard.Core.Enums;
using LeadshineCard.Core.Exceptions;
using LeadshineCard.Core.Interfaces;
using LeadshineCard.Core.Models;
using LeadshineCard.ThirdPart;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace LeadshineCard.Implementation;

/// <summary>
/// 雷赛轴控制器实现
/// </summary>
/// <remarks>
/// 构造函数
/// </remarks>
/// <param name="cardNo">板卡号</param>
/// <param name="axisNo">轴号</param>
/// <param name="logger">日志记录器，为null时使用NullLogger</param>
public class LeadshineAxisController(
    ushort cardNo,
    ushort axisNo,
    ILogger<LeadshineAxisController>? logger = null
) : IAxisController
{
    private readonly ILogger<LeadshineAxisController> _logger =
        logger ?? NullLogger<LeadshineAxisController>.Instance;
    private MotionParameters? _currentParameters;

    public ushort AxisNo => axisNo;

    /// <summary>
    /// 设置运动参数
    /// </summary>
    public async Task SetMotionParametersAsync(MotionParameters parameters)
    {
        ArgumentNullException.ThrowIfNull(parameters);

        parameters.Validate();

        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation(
                "设置轴 {AxisNo} 运动参数: MaxSpeed={MaxSpeed}, Acc={Acceleration}, Dec={Deceleration}",
                axisNo,
                parameters.MaxSpeed,
                parameters.Acceleration,
                parameters.Deceleration
            );
        }

        try
        {
            // 设置脉冲当量
            var result = await Task.Run(
                () => LTDMC.dmc_set_equiv(cardNo, axisNo, parameters.PulseEquivalent)
            );

            if (result != 0)
            {
                throw new AxisException($"设置脉冲当量失败", axisNo, result);
            }

            // 设置速度参数
            result = await Task.Run(
                () =>
                    LTDMC.dmc_set_profile_unit(
                        cardNo,
                        axisNo,
                        parameters.MinSpeed,
                        parameters.MaxSpeed,
                        parameters.Acceleration,
                        parameters.Deceleration,
                        parameters.StopSpeed
                    )
            );

            if (result != 0)
            {
                throw new AxisException($"设置速度参数失败", axisNo, result);
            }

            // 设置S曲线参数
            if (parameters.SCurveTime > 0)
            {
                result = await Task.Run(
                    () => LTDMC.dmc_set_s_profile(cardNo, axisNo, 0, parameters.SCurveTime)
                );

                if (result != 0)
                {
                    if (_logger.IsEnabled(LogLevel.Warning))
                    {
                        _logger.LogWarning("设置S曲线参数失败，错误码: {ErrorCode}", result);
                    }
                }
            }

            _currentParameters = parameters;
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("轴 {AxisNo} 运动参数设置成功", axisNo);
            }
        }
        catch (AxisException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "设置轴 {AxisNo} 运动参数异常", axisNo);
            throw new AxisException($"设置运动参数异常", axisNo, ex);
        }
    }

    /// <summary>
    /// 获取运动参数
    /// </summary>
    public async Task<MotionParameters> GetMotionParametersAsync()
    {
        if (_currentParameters != null)
        {
            return _currentParameters;
        }

        try
        {
            var parameters = new MotionParameters();

            // 获取脉冲当量
            double equiv = 0;
            var result = await Task.Run(() => LTDMC.dmc_get_equiv(cardNo, axisNo, ref equiv));

            if (result == 0)
            {
                parameters.PulseEquivalent = equiv;
            }

            // 获取速度参数
            double minVel = 0,
                maxVel = 0,
                tacc = 0,
                tdec = 0,
                stopVel = 0;
            result = await Task.Run(
                () =>
                    LTDMC.dmc_get_profile_unit(
                        cardNo,
                        axisNo,
                        ref minVel,
                        ref maxVel,
                        ref tacc,
                        ref tdec,
                        ref stopVel
                    )
            );

            if (result == 0)
            {
                parameters.MinSpeed = minVel;
                parameters.MaxSpeed = maxVel;
                parameters.Acceleration = tacc;
                parameters.Deceleration = tdec;
                parameters.StopSpeed = stopVel;
            }

            _currentParameters = parameters;
            return parameters;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取轴 {AxisNo} 运动参数异常", axisNo);
            throw new AxisException($"获取运动参数异常", axisNo, ex);
        }
    }

    /// <summary>
    /// 相对运动
    /// </summary>
    public async Task<bool> MoveRelativeAsync(double distance)
    {
        if (double.IsNaN(distance) || double.IsInfinity(distance))
        {
            throw new ArgumentException("距离参数无效", nameof(distance));
        }

        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("轴 {AxisNo} 相对运动，距离: {Distance}", axisNo, distance);
        }

        try
        {
            var result = await Task.Run(() => LTDMC.dmc_pmove_unit(cardNo, axisNo, distance, 1)); // 1=相对运动

            if (result != 0)
            {
                _logger.LogError("轴 {AxisNo} 相对运动失败，错误码: {ErrorCode}", axisNo, result);
                throw new AxisMotionException($"相对运动失败", axisNo, result);
            }

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("轴 {AxisNo} 相对运动命令发送成功", axisNo);
            }
            return true;
        }
        catch (AxisMotionException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "轴 {AxisNo} 相对运动异常", axisNo);
            throw new AxisMotionException($"相对运动异常", axisNo);
        }
    }

    /// <summary>
    /// 绝对运动
    /// </summary>
    public async Task<bool> MoveAbsoluteAsync(double position)
    {
        if (double.IsNaN(position) || double.IsInfinity(position))
        {
            throw new ArgumentException("位置参数无效", nameof(position));
        }

        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("轴 {AxisNo} 绝对运动，目标位置: {Position}", axisNo, position);
        }

        try
        {
            var result = await Task.Run(() => LTDMC.dmc_pmove_unit(cardNo, axisNo, position, 0)); // 0=绝对运动

            if (result != 0)
            {
                _logger.LogError("轴 {AxisNo} 绝对运动失败，错误码: {ErrorCode}", axisNo, result);
                throw new AxisMotionException($"绝对运动失败", axisNo, result);
            }

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("轴 {AxisNo} 绝对运动命令发送成功", axisNo);
            }
            return true;
        }
        catch (AxisMotionException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "轴 {AxisNo} 绝对运动异常", axisNo);
            throw new AxisMotionException($"绝对运动异常", axisNo);
        }
    }

    /// <summary>
    /// JOG运动
    /// </summary>
    public async Task<bool> JogAsync(bool positiveDirection)
    {
        if (_logger.IsEnabled(LogLevel.Information))
        {
            var direction = positiveDirection ? "正向" : "负向";
            _logger.LogInformation("轴 {AxisNo} JOG运动，方向: {Direction}", axisNo, direction);
        }

        try
        {
            ushort dir = (ushort)(positiveDirection ? 0 : 1);
            var result = await Task.Run(() => LTDMC.dmc_vmove(cardNo, axisNo, dir));

            if (result != 0)
            {
                _logger.LogError("轴 {AxisNo} JOG运动失败，错误码: {ErrorCode}", axisNo, result);
                throw new AxisMotionException($"JOG运动失败", axisNo, result);
            }

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("轴 {AxisNo} JOG运动命令发送成功", axisNo);
            }
            return true;
        }
        catch (AxisMotionException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "轴 {AxisNo} JOG运动异常", axisNo);
            throw new AxisMotionException($"JOG运动异常", axisNo);
        }
    }

    /// <summary>
    /// 停止运动
    /// </summary>
    public async Task<bool> StopAsync(StopMode mode)
    {
        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("轴 {AxisNo} 停止运动，模式: {Mode}", axisNo, mode);
        }

        try
        {
            ushort stopMode = mode switch
            {
                StopMode.Immediate => 0,
                StopMode.Deceleration => 1,
                StopMode.Emergency => 2,
                _ => 1,
            };

            var result = await Task.Run(() => LTDMC.dmc_stop(cardNo, axisNo, stopMode));

            if (result != 0)
            {
                _logger.LogError("轴 {AxisNo} 停止失败，错误码: {ErrorCode}", axisNo, result);
                return false;
            }

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("轴 {AxisNo} 停止命令发送成功", axisNo);
            }
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "轴 {AxisNo} 停止异常", axisNo);
            throw new AxisException($"停止异常", axisNo, ex);
        }
    }

    /// <summary>
    /// 获取当前位置
    /// </summary>
    public async Task<double> GetCurrentPositionAsync()
    {
        try
        {
            double position = 0;
            var result = await Task.Run(
                () => LTDMC.dmc_get_position_unit(cardNo, axisNo, ref position)
            );

            if (result != 0)
            {
                if (_logger.IsEnabled(LogLevel.Warning))
                {
                    _logger.LogWarning("获取轴 {AxisNo} 位置失败，错误码: {ErrorCode}", axisNo, result);
                }
                return 0;
            }

            return position;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取轴 {AxisNo} 位置异常", axisNo);
            return 0;
        }
    }

    /// <summary>
    /// 设置当前位置
    /// </summary>
    public async Task<bool> SetCurrentPositionAsync(double position)
    {
        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("设置轴 {AxisNo} 当前位置: {Position}", axisNo, position);
        }

        try
        {
            var result = await Task.Run(
                () => LTDMC.dmc_set_position_unit(cardNo, axisNo, position)
            );

            if (result != 0)
            {
                _logger.LogError("设置轴 {AxisNo} 位置失败，错误码: {ErrorCode}", axisNo, result);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "设置轴 {AxisNo} 位置异常", axisNo);
            throw new AxisException($"设置位置异常", axisNo, ex);
        }
    }

    /// <summary>
    /// 获取当前速度
    /// </summary>
    public async Task<double> GetCurrentSpeedAsync()
    {
        try
        {
            double speed = 0;
            var result = await Task.Run(
                () => LTDMC.dmc_read_current_speed_unit(cardNo, axisNo, ref speed)
            );

            if (result != 0)
            {
                if (_logger.IsEnabled(LogLevel.Warning))
                {
                    _logger.LogWarning("获取轴 {AxisNo} 速度失败，错误码: {ErrorCode}", axisNo, result);
                }
                return 0;
            }

            return speed;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取轴 {AxisNo} 速度异常", axisNo);
            return 0;
        }
    }

    /// <summary>
    /// 获取目标位置
    /// </summary>
    public async Task<double> GetTargetPositionAsync()
    {
        try
        {
            double targetPos = 0;
            var result = await Task.Run(
                () => LTDMC.dmc_get_target_position_unit(cardNo, axisNo, ref targetPos)
            );

            if (result != 0)
            {
                if (_logger.IsEnabled(LogLevel.Warning))
                {
                    _logger.LogWarning(
                        "获取轴 {AxisNo} 目标位置失败，错误码: {ErrorCode}",
                        axisNo,
                        result
                    );
                }
                return 0;
            }

            return targetPos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取轴 {AxisNo} 目标位置异常", axisNo);
            return 0;
        }
    }

    /// <summary>
    /// 获取轴状态
    /// </summary>
    public async Task<AxisStatus> GetStatusAsync()
    {
        try
        {
            var status = new AxisStatus
            {
                AxisNo = axisNo,
                Timestamp = DateTime.Now, // 获取位置
                Position = await GetCurrentPositionAsync(),

                // 获取速度
                Speed = await GetCurrentSpeedAsync(),

                // 获取目标位置
                TargetPosition = await GetTargetPositionAsync(),
            };

            // 获取IO状态
            var ioStatus = await Task.Run(() => LTDMC.dmc_axis_io_status(cardNo, axisNo));
            status.PositiveLimit = (ioStatus & 0x01) != 0;
            status.NegativeLimit = (ioStatus & 0x02) != 0;
            status.Home = (ioStatus & 0x04) != 0;
            status.Alarm = (ioStatus & 0x08) != 0;

            // 检查运动状态
            status.Done = await CheckDoneAsync();
            status.State = status.Done ? AxisState.Idle : AxisState.Moving;

            return status;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取轴 {AxisNo} 状态异常", axisNo);
            throw new AxisException($"获取状态异常", axisNo, ex);
        }
    }

    /// <summary>
    /// 检查运动是否完成
    /// </summary>
    public async Task<bool> CheckDoneAsync()
    {
        try
        {
            var result = await Task.Run(() => LTDMC.dmc_check_done(cardNo, axisNo));
            return result == 1; // 1表示运动完成
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "检查轴 {AxisNo} 运动状态异常", axisNo);
            return false;
        }
    }

    /// <summary>
    /// 在线变速
    /// </summary>
    public async Task<bool> ChangeSpeedAsync(double newSpeed, double accelTime)
    {
        if (newSpeed <= 0)
        {
            throw new ArgumentException("速度必须大于0", nameof(newSpeed));
        }

        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation(
                "轴 {AxisNo} 在线变速，新速度: {NewSpeed}, 加速时间: {AccelTime}",
                axisNo,
                newSpeed,
                accelTime
            );
        }

        try
        {
            var result = await Task.Run(
                () => LTDMC.dmc_change_speed_unit(cardNo, axisNo, newSpeed, accelTime)
            );

            if (result != 0)
            {
                _logger.LogError("轴 {AxisNo} 在线变速失败，错误码: {ErrorCode}", axisNo, result);
                return false;
            }

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("轴 {AxisNo} 在线变速命令发送成功", axisNo);
            }
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "轴 {AxisNo} 在线变速异常", axisNo);
            throw new AxisException($"在线变速异常", axisNo, ex);
        }
    }
}
