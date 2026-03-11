using LeadshineCard.Core.Interfaces;
using LeadshineCard.ThirdPart;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace LeadshineCard.Implementation;

/// <summary>
/// 雷赛插补控制器实现
/// </summary>
/// <remarks>
/// 构造函数
/// </remarks>
/// <param name="cardNo">板卡号</param>
/// <param name="logger">日志记录器，为null时使用NullLogger</param>
public class LeadshineInterpolationController(
    ushort cardNo,
    ILogger<LeadshineInterpolationController>? logger = null
) : IInterpolationController
{
    private readonly ILogger<LeadshineInterpolationController> _logger =
        logger ?? NullLogger<LeadshineInterpolationController>.Instance;

    /// <summary>
    /// 直线插补
    /// </summary>
    public async Task<bool> LineInterpolationAsync(ushort[] axes, double[] targetPositions)
    {
        if (axes == null || axes.Length == 0)
            throw new ArgumentException("轴数组不能为空", nameof(axes));

        if (targetPositions == null || targetPositions.Length != axes.Length)
            throw new ArgumentException(
                "目标位置数组长度必须与轴数组相同",
                nameof(targetPositions)
            );

        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("直线插补，轴数: {AxisCount}", axes.Length);
        }

        try
        {
            var result = await Task.Run(
                () => LTDMC.dmc_line_unit(cardNo, 0, (ushort)axes.Length, axes, targetPositions, 0)
            );

            if (result != 0)
            {
                _logger.LogError("直线插补失败，错误码: {ErrorCode}", result);
                return false;
            }

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("直线插补命令发送成功");
            }
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "直线插补异常");
            return false;
        }
    }

    /// <summary>
    /// 圆弧插补
    /// </summary>
    public async Task<bool> ArcInterpolationAsync(
        ushort[] axes,
        double[] targetPositions,
        double[] centerPositions,
        bool clockwise
    )
    {
        if (axes == null || axes.Length < 2)
            throw new ArgumentException("圆弧插补至少需要2个轴", nameof(axes));

        if (targetPositions == null || targetPositions.Length != axes.Length)
            throw new ArgumentException(
                "目标位置数组长度必须与轴数组相同",
                nameof(targetPositions)
            );

        if (centerPositions == null || centerPositions.Length != axes.Length)
            throw new ArgumentException(
                "圆心位置数组长度必须与轴数组相同",
                nameof(centerPositions)
            );

        if (_logger.IsEnabled(LogLevel.Information))
        {
            var direction = clockwise ? "顺时针" : "逆时针";
            _logger.LogInformation(
                "圆弧插补，轴数: {AxisCount}, 方向: {Direction}",
                axes.Length,
                direction
            );
        }

        try
        {
            ushort arcDir = (ushort)(clockwise ? 0 : 1);
            var result = await Task.Run(
                () =>
                    LTDMC.dmc_arc_move_center_unit(
                        cardNo,
                        0,
                        (ushort)axes.Length,
                        axes,
                        targetPositions,
                        centerPositions,
                        arcDir,
                        0,
                        0
                    )
            );

            if (result != 0)
            {
                _logger.LogError("圆弧插补失败，错误码: {ErrorCode}", result);
                return false;
            }

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("圆弧插补命令发送成功");
            }
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "圆弧插补异常");
            return false;
        }
    }

    /// <summary>
    /// 打开连续插补缓冲区
    /// </summary>
    public async Task<bool> OpenContinuousBufferAsync(ushort crd, ushort[] axes)
    {
        if (axes == null || axes.Length == 0)
            throw new ArgumentException("轴数组不能为空", nameof(axes));

        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation(
                "打开坐标系 {Crd} 连续插补缓冲区，轴数: {AxisCount}",
                crd,
                axes.Length
            );
        }

        try
        {
            var result = await Task.Run(
                () => LTDMC.dmc_conti_open_list(cardNo, crd, (ushort)axes.Length, axes)
            );

            if (result != 0)
            {
                _logger.LogError("打开连续插补缓冲区失败，错误码: {ErrorCode}", result);
                return false;
            }

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("连续插补缓冲区已打开");
            }
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "打开连续插补缓冲区异常");
            return false;
        }
    }

    /// <summary>
    /// 关闭连续插补缓冲区
    /// </summary>
    public async Task<bool> CloseContinuousBufferAsync(ushort crd)
    {
        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("关闭坐标系 {Crd} 连续插补缓冲区", crd);
        }

        try
        {
            var result = await Task.Run(() => LTDMC.dmc_conti_close_list(cardNo, crd));

            if (result != 0)
            {
                _logger.LogError("关闭连续插补缓冲区失败，错误码: {ErrorCode}", result);
                return false;
            }

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("连续插补缓冲区已关闭");
            }
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "关闭连续插补缓冲区异常");
            return false;
        }
    }

    /// <summary>
    /// 添加直线段
    /// </summary>
    public async Task<bool> AddLineSegmentAsync(ushort crd, double[] targetPositions, int mark = 0)
    {
        if (targetPositions == null || targetPositions.Length == 0)
            throw new ArgumentException("目标位置数组不能为空", nameof(targetPositions));

        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("添加直线段到坐标系 {Crd}，标号: {Mark}", crd, mark);
        }

        try
        {
            // 获取坐标系的轴列表（这里简化处理，实际应该从配置获取）
            var axes = new ushort[targetPositions.Length];
            for (ushort i = 0; i < targetPositions.Length; i++)
            {
                axes[i] = i;
            }

            var result = await Task.Run(
                () =>
                    LTDMC.dmc_conti_line_unit(
                        cardNo,
                        crd,
                        (ushort)axes.Length,
                        axes,
                        targetPositions,
                        0,
                        mark
                    )
            );

            if (result != 0)
            {
                _logger.LogError("添加直线段失败，错误码: {ErrorCode}", result);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "添加直线段异常");
            return false;
        }
    }

    /// <summary>
    /// 添加圆弧段
    /// </summary>
    public async Task<bool> AddArcSegmentAsync(
        ushort crd,
        double[] targetPositions,
        double[] centerPositions,
        bool clockwise,
        int mark = 0
    )
    {
        if (targetPositions == null || targetPositions.Length < 2)
            throw new ArgumentException("圆弧至少需要2个轴的目标位置", nameof(targetPositions));

        if (centerPositions == null || centerPositions.Length != targetPositions.Length)
            throw new ArgumentException(
                "圆心位置数组长度必须与目标位置数组相同",
                nameof(centerPositions)
            );

        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("添加圆弧段到坐标系 {Crd}，标号: {Mark}", crd, mark);
        }

        try
        {
            var axes = new ushort[targetPositions.Length];
            for (ushort i = 0; i < targetPositions.Length; i++)
            {
                axes[i] = i;
            }

            ushort arcDir = (ushort)(clockwise ? 0 : 1);
            var result = await Task.Run(
                () =>
                    LTDMC.dmc_conti_arc_move_center_unit(
                        cardNo,
                        crd,
                        (ushort)axes.Length,
                        axes,
                        targetPositions,
                        centerPositions,
                        arcDir,
                        0,
                        0,
                        mark
                    )
            );

            if (result != 0)
            {
                _logger.LogError("添加圆弧段失败，错误码: {ErrorCode}", result);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "添加圆弧段异常");
            return false;
        }
    }

    /// <summary>
    /// 开始连续插补
    /// </summary>
    public async Task<bool> StartContinuousAsync(ushort crd)
    {
        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("开始坐标系 {Crd} 连续插补", crd);
        }

        try
        {
            var result = await Task.Run(() => LTDMC.dmc_conti_start_list(cardNo, crd));

            if (result != 0)
            {
                _logger.LogError("开始连续插补失败，错误码: {ErrorCode}", result);
                return false;
            }

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("连续插补已开始");
            }
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "开始连续插补异常");
            return false;
        }
    }

    /// <summary>
    /// 暂停连续插补
    /// </summary>
    public async Task<bool> PauseContinuousAsync(ushort crd)
    {
        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("暂停坐标系 {Crd} 连续插补", crd);
        }

        try
        {
            var result = await Task.Run(() => LTDMC.dmc_conti_pause_list(cardNo, crd));

            if (result != 0)
            {
                _logger.LogError("暂停连续插补失败，错误码: {ErrorCode}", result);
                return false;
            }

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("连续插补已暂停");
            }
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "暂停连续插补异常");
            return false;
        }
    }

    /// <summary>
    /// 停止连续插补
    /// </summary>
    public async Task<bool> StopContinuousAsync(ushort crd)
    {
        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("停止坐标系 {Crd} 连续插补", crd);
        }

        try
        {
            var result = await Task.Run(() => LTDMC.dmc_conti_stop_list(cardNo, crd, 1));

            if (result != 0)
            {
                _logger.LogError("停止连续插补失败，错误码: {ErrorCode}", result);
                return false;
            }

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("连续插补已停止");
            }
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "停止连续插补异常");
            return false;
        }
    }

    /// <summary>
    /// 获取剩余缓冲区空间
    /// </summary>
    public async Task<int> GetRemainingBufferSpaceAsync(ushort crd)
    {
        try
        {
            var result = await Task.Run(() => LTDMC.dmc_conti_remain_space(cardNo, crd));
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取剩余缓冲区空间异常");
            return 0;
        }
    }

    /// <summary>
    /// 获取当前段标号
    /// </summary>
    public async Task<int> GetCurrentMarkAsync(ushort crd)
    {
        try
        {
            var result = await Task.Run(() => LTDMC.dmc_conti_read_current_mark(cardNo, crd));
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取当前段标号异常");
            return 0;
        }
    }

    /// <summary>
    /// 检查连续插补是否完成
    /// </summary>
    public async Task<bool> CheckContinuousDoneAsync(ushort crd)
    {
        try
        {
            var result = await Task.Run(() => LTDMC.dmc_conti_check_done(cardNo, crd));
            return result == 1;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "检查连续插补状态异常");
            return false;
        }
    }
}
