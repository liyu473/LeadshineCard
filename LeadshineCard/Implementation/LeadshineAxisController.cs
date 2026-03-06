using LeadshineCard.Core.Enums;
using LeadshineCard.Core.Exceptions;
using LeadshineCard.Core.Interfaces;
using LeadshineCard.Core.Models;
using LeadshineCard.ThirdPart;
using Microsoft.Extensions.Logging;

namespace LeadshineCard.Implementation;

/// <summary>
/// 雷赛轴控制器实现
/// </summary>
public class LeadshineAxisController : IAxisController
{
    private readonly ushort _cardNo;
    private readonly ushort _axisNo;
    private readonly ILogger<LeadshineAxisController> _logger;
    private MotionParameters? _currentParameters;

    public ushort AxisNo => _axisNo;

    /// <summary>
    /// 构造函数
    /// </summary>
    public LeadshineAxisController(
        ushort cardNo,
        ushort axisNo,
        ILogger<LeadshineAxisController> logger)
    {
        _cardNo = cardNo;
        _axisNo = axisNo;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// 设置运动参数
    /// </summary>
    public async Task SetMotionParametersAsync(MotionParameters parameters)
    {
        ArgumentNullException.ThrowIfNull(parameters);

        parameters.Validate();

        _logger.LogInformation(
            "设置轴 {AxisNo} 运动参数: MaxSpeed={MaxSpeed}, Acc={Acceleration}, Dec={Deceleration}",
            _axisNo, parameters.MaxSpeed, parameters.Acceleration, parameters.Deceleration);

        try
        {
            // 设置脉冲当量
            var result = await Task.Run(() =>
                LTDMC.dmc_set_equiv(_cardNo, _axisNo, parameters.PulseEquivalent));

            if (result != 0)
            {
                throw new AxisException($"设置脉冲当量失败", _axisNo, result);
            }

            // 设置速度参数
            result = await Task.Run(() =>
                LTDMC.dmc_set_profile_unit(_cardNo, _axisNo,
                    parameters.MinSpeed,
                    parameters.MaxSpeed,
                    parameters.Acceleration,
                    parameters.Deceleration,
                    parameters.StopSpeed));

            if (result != 0)
            {
                throw new AxisException($"设置速度参数失败", _axisNo, result);
            }

            // 设置S曲线参数
            if (parameters.SCurveTime > 0)
            {
                result = await Task.Run(() =>
                    LTDMC.dmc_set_s_profile(_cardNo, _axisNo, 0, parameters.SCurveTime));

                if (result != 0)
                {
                    _logger.LogWarning("设置S曲线参数失败，错误码: {ErrorCode}", result);
                }
            }

            _currentParameters = parameters;
            _logger.LogDebug("轴 {AxisNo} 运动参数设置成功", _axisNo);
        }
        catch (AxisException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "设置轴 {AxisNo} 运动参数异常", _axisNo);
            throw new AxisException($"设置运动参数异常", _axisNo, ex);
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
            var result = await Task.Run(() =>
                LTDMC.dmc_get_equiv(_cardNo, _axisNo, ref equiv));

            if (result == 0)
            {
                parameters.PulseEquivalent = equiv;
            }

            // 获取速度参数
            double minVel = 0, maxVel = 0, tacc = 0, tdec = 0, stopVel = 0;
            result = await Task.Run(() =>
                LTDMC.dmc_get_profile_unit(_cardNo, _axisNo,
                    ref minVel, ref maxVel, ref tacc, ref tdec, ref stopVel));

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
            _logger.LogError(ex, "获取轴 {AxisNo} 运动参数异常", _axisNo);
            throw new AxisException($"获取运动参数异常", _axisNo, ex);
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

        _logger.LogInformation("轴 {AxisNo} 相对运动，距离: {Distance}", _axisNo, distance);

        try
        {
            var result = await Task.Run(() =>
                LTDMC.dmc_pmove_unit(_cardNo, _axisNo, distance, 1)); // 1=相对运动

            if (result != 0)
            {
                _logger.LogError("轴 {AxisNo} 相对运动失败，错误码: {ErrorCode}", _axisNo, result);
                throw new AxisMotionException($"相对运动失败", _axisNo, result);
            }

            _logger.LogDebug("轴 {AxisNo} 相对运动命令发送成功", _axisNo);
            return true;
        }
        catch (AxisMotionException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "轴 {AxisNo} 相对运动异常", _axisNo);
            throw new AxisMotionException($"相对运动异常", _axisNo);
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

        _logger.LogInformation("轴 {AxisNo} 绝对运动，目标位置: {Position}", _axisNo, position);

        try
        {
            var result = await Task.Run(() =>
                LTDMC.dmc_pmove_unit(_cardNo, _axisNo, position, 0)); // 0=绝对运动

            if (result != 0)
            {
                _logger.LogError("轴 {AxisNo} 绝对运动失败，错误码: {ErrorCode}", _axisNo, result);
                throw new AxisMotionException($"绝对运动失败", _axisNo, result);
            }

            _logger.LogDebug("轴 {AxisNo} 绝对运动命令发送成功", _axisNo);
            return true;
        }
        catch (AxisMotionException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "轴 {AxisNo} 绝对运动异常", _axisNo);
            throw new AxisMotionException($"绝对运动异常", _axisNo);
        }
    }

    /// <summary>
    /// JOG运动
    /// </summary>
    public async Task<bool> JogAsync(bool positiveDirection)
    {
        var direction = positiveDirection ? "正向" : "负向";
        _logger.LogInformation("轴 {AxisNo} JOG运动，方向: {Direction}", _axisNo, direction);

        try
        {
            ushort dir = (ushort)(positiveDirection ? 0 : 1);
            var result = await Task.Run(() =>
                LTDMC.dmc_vmove(_cardNo, _axisNo, dir));

            if (result != 0)
            {
                _logger.LogError("轴 {AxisNo} JOG运动失败，错误码: {ErrorCode}", _axisNo, result);
                throw new AxisMotionException($"JOG运动失败", _axisNo, result);
            }

            _logger.LogDebug("轴 {AxisNo} JOG运动命令发送成功", _axisNo);
            return true;
        }
        catch (AxisMotionException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "轴 {AxisNo} JOG运动异常", _axisNo);
            throw new AxisMotionException($"JOG运动异常", _axisNo);
        }
    }

    /// <summary>
    /// 停止运动
    /// </summary>
    public async Task<bool> StopAsync(StopMode mode)
    {
        _logger.LogInformation("轴 {AxisNo} 停止运动，模式: {Mode}", _axisNo, mode);

        try
        {
            ushort stopMode = mode switch
            {
                StopMode.Immediate => 0,
                StopMode.Deceleration => 1,
                StopMode.Emergency => 2,
                _ => 1
            };

            var result = await Task.Run(() =>
                LTDMC.dmc_stop(_cardNo, _axisNo, stopMode));

            if (result != 0)
            {
                _logger.LogError("轴 {AxisNo} 停止失败，错误码: {ErrorCode}", _axisNo, result);
                return false;
            }

            _logger.LogDebug("轴 {AxisNo} 停止命令发送成功", _axisNo);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "轴 {AxisNo} 停止异常", _axisNo);
            throw new AxisException($"停止异常", _axisNo, ex);
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
            var result = await Task.Run(() =>
                LTDMC.dmc_get_position_unit(_cardNo, _axisNo, ref position));

            if (result != 0)
            {
                _logger.LogWarning("获取轴 {AxisNo} 位置失败，错误码: {ErrorCode}", _axisNo, result);
                return 0;
            }

            return position;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取轴 {AxisNo} 位置异常", _axisNo);
            return 0;
        }
    }

    /// <summary>
    /// 设置当前位置
    /// </summary>
    public async Task<bool> SetCurrentPositionAsync(double position)
    {
        _logger.LogInformation("设置轴 {AxisNo} 当前位置: {Position}", _axisNo, position);

        try
        {
            var result = await Task.Run(() =>
                LTDMC.dmc_set_position_unit(_cardNo, _axisNo, position));

            if (result != 0)
            {
                _logger.LogError("设置轴 {AxisNo} 位置失败，错误码: {ErrorCode}", _axisNo, result);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "设置轴 {AxisNo} 位置异常", _axisNo);
            throw new AxisException($"设置位置异常", _axisNo, ex);
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
            var result = await Task.Run(() =>
                LTDMC.dmc_read_current_speed_unit(_cardNo, _axisNo, ref speed));

            if (result != 0)
            {
                _logger.LogWarning("获取轴 {AxisNo} 速度失败，错误码: {ErrorCode}", _axisNo, result);
                return 0;
            }

            return speed;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取轴 {AxisNo} 速度异常", _axisNo);
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
            var result = await Task.Run(() =>
                LTDMC.dmc_get_target_position_unit(_cardNo, _axisNo, ref targetPos));

            if (result != 0)
            {
                _logger.LogWarning("获取轴 {AxisNo} 目标位置失败，错误码: {ErrorCode}", _axisNo, result);
                return 0;
            }

            return targetPos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取轴 {AxisNo} 目标位置异常", _axisNo);
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
                AxisNo = _axisNo,
                Timestamp = DateTime.Now
            };

            // 获取位置
            status.Position = await GetCurrentPositionAsync();

            // 获取速度
            status.Speed = await GetCurrentSpeedAsync();

            // 获取目标位置
            status.TargetPosition = await GetTargetPositionAsync();

            // 获取IO状态
            var ioStatus = await Task.Run(() => LTDMC.dmc_axis_io_status(_cardNo, _axisNo));
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
            _logger.LogError(ex, "获取轴 {AxisNo} 状态异常", _axisNo);
            throw new AxisException($"获取状态异常", _axisNo, ex);
        }
    }

    /// <summary>
    /// 检查运动是否完成
    /// </summary>
    public async Task<bool> CheckDoneAsync()
    {
        try
        {
            var result = await Task.Run(() => LTDMC.dmc_check_done(_cardNo, _axisNo));
            return result == 1; // 1表示运动完成
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "检查轴 {AxisNo} 运动状态异常", _axisNo);
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

        _logger.LogInformation("轴 {AxisNo} 在线变速，新速度: {NewSpeed}, 加速时间: {AccelTime}",
            _axisNo, newSpeed, accelTime);

        try
        {
            var result = await Task.Run(() =>
                LTDMC.dmc_change_speed_unit(_cardNo, _axisNo, newSpeed, accelTime));

            if (result != 0)
            {
                _logger.LogError("轴 {AxisNo} 在线变速失败，错误码: {ErrorCode}", _axisNo, result);
                return false;
            }

            _logger.LogDebug("轴 {AxisNo} 在线变速命令发送成功", _axisNo);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "轴 {AxisNo} 在线变速异常", _axisNo);
            throw new AxisException($"在线变速异常", _axisNo, ex);
        }
    }
}
