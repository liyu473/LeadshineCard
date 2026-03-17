using LeadshineCard.Core.Events;
using LeadshineCard.Core.Exceptions;
using LeadshineCard.Core.Helpers;
using LeadshineCard.Core.Interfaces;
using LeadshineCard.Core.Models;
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
    private readonly Dictionary<ushort, InterpolationParameters> _parametersCache = [];
    private const int MinBufferSpace = 10; // 最小缓冲区空间阈值

    // 事件定义
    public event EventHandler<InterpolationCompletedEventArgs>? InterpolationCompleted;
    public event EventHandler<SegmentCompletedEventArgs>? SegmentCompleted;
    public event EventHandler<BufferStatusEventArgs>? BufferLow;

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
        _logger.LogInformation("直线插补，轴数: {AxisCount}", axes.Length);

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
            _logger.LogDebug("直线插补命令发送成功");
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
            _logger.LogDebug("圆弧插补命令发送成功");
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
            _logger.LogDebug("连续插补缓冲区已打开");
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
        _logger.LogInformation("关闭坐标系 {Crd} 连续插补缓冲区", crd);

        try
        {
            var result = await Task.Run(() => LTDMC.dmc_conti_close_list(cardNo, crd));

            if (result != 0)
            {
                _logger.LogError("关闭连续插补缓冲区失败，错误码: {ErrorCode}", result);
                return false;
            }
            _logger.LogDebug("连续插补缓冲区已关闭");
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

        // 自动检查并等待缓冲区空间
        await EnsureBufferSpaceAsync(crd, 1);
        _logger.LogDebug("添加直线段到坐标系 {Crd}，标号: {Mark}", crd, mark);

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
                throw new MotionCardException($"添加直线段失败", result);
            }

            return true;
        }
        catch (MotionCardException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "添加直线段异常");
            throw new MotionCardException("添加直线段异常", ex);
        }
    }

    /// <summary>
    /// 确保缓冲区有足够空间
    /// </summary>
    private async Task EnsureBufferSpaceAsync(ushort crd, int requiredSpace, int maxWaitMs = 5000)
    {
        var startTime = DateTime.Now;
        var timeout = TimeSpan.FromMilliseconds(maxWaitMs);

        while (true)
        {
            var remainSpace = await GetRemainingBufferSpaceAsync(crd);

            if (remainSpace >= requiredSpace)
            {
                // 检查是否需要触发缓冲区低事件
                if (remainSpace < MinBufferSpace)
                {
                    BufferLow?.Invoke(
                        this,
                        new BufferStatusEventArgs
                        {
                            CoordinateSystem = crd,
                            RemainingSpace = remainSpace,
                            IsLow = true,
                        }
                    );
                }
                return;
            }

            if (DateTime.Now - startTime > timeout)
            {
                throw new MotionCardException(
                    $"插补缓冲区空间不足: 需要 {requiredSpace}，剩余 {remainSpace}"
                );
            }

            _logger.LogDebug(
                "坐标系 {Crd} 缓冲区空间不足，等待中... 需要: {Required}, 剩余: {Remain}",
                crd,
                requiredSpace,
                remainSpace
            );

            await Task.Delay(100);
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

        // 自动检查并等待缓冲区空间
        await EnsureBufferSpaceAsync(crd, 1);
        _logger.LogDebug("添加圆弧段到坐标系 {Crd}，标号: {Mark}", crd, mark);

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
                throw new MotionCardException($"添加圆弧段失败", result);
            }

            return true;
        }
        catch (MotionCardException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "添加圆弧段异常");
            throw new MotionCardException("添加圆弧段异常", ex);
        }
    }

    /// <summary>
    /// 开始连续插补
    /// </summary>
    public async Task<bool> StartContinuousAsync(ushort crd)
    {
        _logger.LogInformation("开始坐标系 {Crd} 连续插补", crd);

        try
        {
            var result = await Task.Run(() => LTDMC.dmc_conti_start_list(cardNo, crd));

            if (result != 0)
            {
                _logger.LogError("开始连续插补失败，错误码: {ErrorCode}", result);
                return false;
            }
            _logger.LogDebug("连续插补已开始");
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
        _logger.LogInformation("暂停坐标系 {Crd} 连续插补", crd);

        try
        {
            var result = await Task.Run(() => LTDMC.dmc_conti_pause_list(cardNo, crd));

            if (result != 0)
            {
                _logger.LogError("暂停连续插补失败，错误码: {ErrorCode}", result);
                return false;
            }
            _logger.LogDebug("连续插补已暂停");
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
        _logger.LogInformation("停止坐标系 {Crd} 连续插补", crd);

        try
        {
            var result = await Task.Run(() => LTDMC.dmc_conti_stop_list(cardNo, crd, 1));

            if (result != 0)
            {
                _logger.LogError("停止连续插补失败，错误码: {ErrorCode}", result);
                return false;
            }
            _logger.LogDebug("连续插补已停止");
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

    /// <summary>
    /// 设置插补速度参数
    /// </summary>
    public async Task<bool> SetVectorProfileAsync(
        ushort crd,
        double minVel,
        double maxVel,
        double tacc,
        double tdec,
        double stopVel
    )
    {
        _logger.LogInformation(
            "设置坐标系 {Crd} 插补速度参数: MinVel={MinVel}, MaxVel={MaxVel}, Tacc={Tacc}, Tdec={Tdec}, StopVel={StopVel}",
            crd,
            minVel,
            maxVel,
            tacc,
            tdec,
            stopVel
        );

        try
        {
            var result = await Task.Run(
                () =>
                    LTDMC.dmc_set_vector_profile_unit(
                        cardNo,
                        crd,
                        minVel,
                        maxVel,
                        tacc,
                        tdec,
                        stopVel
                    )
            );

            if (result != 0)
            {
                _logger.LogError("设置插补速度参数失败，错误码: {ErrorCode}", result);
                return false;
            }
            _logger.LogDebug("插补速度参数设置成功");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "设置插补速度参数异常");
            return false;
        }
    }

    /// <summary>
    /// 获取插补速度参数
    /// </summary>
    public async Task<(
        double minVel,
        double maxVel,
        double tacc,
        double tdec,
        double stopVel
    )?> GetVectorProfileAsync(ushort crd)
    {
        try
        {
            double minVel = 0,
                maxVel = 0,
                tacc = 0,
                tdec = 0,
                stopVel = 0;

            var result = await Task.Run(
                () =>
                    LTDMC.dmc_get_vector_profile_unit(
                        cardNo,
                        crd,
                        ref minVel,
                        ref maxVel,
                        ref tacc,
                        ref tdec,
                        ref stopVel
                    )
            );

            if (result != 0)
            {
                _logger.LogError("获取插补速度参数失败，错误码: {ErrorCode}", result);
                return null;
            }

            return (minVel, maxVel, tacc, tdec, stopVel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取插补速度参数异常");
            return null;
        }
    }

    /// <summary>
    /// 圆弧插补（半径终点式）
    /// </summary>
    public async Task<bool> ArcInterpolationByRadiusAsync(
        ushort[] axes,
        double[] targetPositions,
        double arcRadius,
        bool clockwise,
        int circle = 0
    )
    {
        if (axes == null || axes.Length < 2)
            throw new ArgumentException("圆弧插补至少需要2个轴", nameof(axes));

        if (targetPositions == null || targetPositions.Length != axes.Length)
            throw new ArgumentException(
                "目标位置数组长度必须与轴数组相同",
                nameof(targetPositions)
            );

        var direction = clockwise ? "顺时针" : "逆时针";
        _logger.LogInformation(
            "半径式圆弧插补，轴数: {AxisCount}, 半径: {Radius}, 方向: {Direction}",
            axes.Length,
            arcRadius,
            direction
        );

        try
        {
            ushort arcDir = (ushort)(clockwise ? 0 : 1);
            var result = await Task.Run(
                () =>
                    LTDMC.dmc_arc_move_radius_unit(
                        cardNo,
                        0,
                        (ushort)axes.Length,
                        axes,
                        targetPositions,
                        arcRadius,
                        arcDir,
                        circle,
                        0
                    )
            );

            if (result != 0)
            {
                _logger.LogError("半径式圆弧插补失败，错误码: {ErrorCode}", result);
                return false;
            }
            _logger.LogDebug("半径式圆弧插补命令发送成功");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "半径式圆弧插补异常");
            return false;
        }
    }

    /// <summary>
    /// 圆弧插补（三点式）
    /// </summary>
    public async Task<bool> ArcInterpolationBy3PointsAsync(
        ushort[] axes,
        double[] targetPositions,
        double[] midPositions,
        int circle = 0
    )
    {
        if (axes == null || axes.Length < 2)
            throw new ArgumentException("圆弧插补至少需要2个轴", nameof(axes));

        if (targetPositions == null || targetPositions.Length != axes.Length)
            throw new ArgumentException(
                "目标位置数组长度必须与轴数组相同",
                nameof(targetPositions)
            );

        if (midPositions == null || midPositions.Length != axes.Length)
            throw new ArgumentException("中间点位置数组长度必须与轴数组相同", nameof(midPositions));
        _logger.LogInformation("三点式圆弧插补，轴数: {AxisCount}", axes.Length);

        try
        {
            var result = await Task.Run(
                () =>
                    LTDMC.dmc_arc_move_3points_unit(
                        cardNo,
                        0,
                        (ushort)axes.Length,
                        axes,
                        targetPositions,
                        midPositions,
                        circle,
                        0
                    )
            );

            if (result != 0)
            {
                _logger.LogError("三点式圆弧插补失败，错误码: {ErrorCode}", result);
                return false;
            }
            _logger.LogDebug("三点式圆弧插补命令发送成功");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "三点式圆弧插补异常");
            return false;
        }
    }

    /// <summary>
    /// 添加圆弧段到连续插补缓冲区（半径式）
    /// </summary>
    public async Task<bool> AddArcSegmentByRadiusAsync(
        ushort crd,
        double[] targetPositions,
        double arcRadius,
        bool clockwise,
        int circle = 0,
        int mark = 0
    )
    {
        if (targetPositions == null || targetPositions.Length < 2)
            throw new ArgumentException("圆弧至少需要2个轴的目标位置", nameof(targetPositions));
        _logger.LogDebug("添加半径式圆弧段到坐标系 {Crd}，标号: {Mark}", crd, mark);

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
                    LTDMC.dmc_conti_arc_move_radius_unit(
                        cardNo,
                        crd,
                        (ushort)axes.Length,
                        axes,
                        targetPositions,
                        arcRadius,
                        arcDir,
                        circle,
                        0,
                        mark
                    )
            );

            if (result != 0)
            {
                _logger.LogError("添加半径式圆弧段失败，错误码: {ErrorCode}", result);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "添加半径式圆弧段异常");
            return false;
        }
    }

    /// <summary>
    /// 添加圆弧段到连续插补缓冲区（三点式）
    /// </summary>
    public async Task<bool> AddArcSegmentBy3PointsAsync(
        ushort crd,
        double[] targetPositions,
        double[] midPositions,
        int circle = 0,
        int mark = 0
    )
    {
        if (targetPositions == null || targetPositions.Length < 2)
            throw new ArgumentException("圆弧至少需要2个轴的目标位置", nameof(targetPositions));

        if (midPositions == null || midPositions.Length != targetPositions.Length)
            throw new ArgumentException(
                "中间点位置数组长度必须与目标位置数组相同",
                nameof(midPositions)
            );
        _logger.LogDebug("添加三点式圆弧段到坐标系 {Crd}，标号: {Mark}", crd, mark);

        try
        {
            var axes = new ushort[targetPositions.Length];
            for (ushort i = 0; i < targetPositions.Length; i++)
            {
                axes[i] = i;
            }

            var result = await Task.Run(
                () =>
                    LTDMC.dmc_conti_arc_move_3points_unit(
                        cardNo,
                        crd,
                        (ushort)axes.Length,
                        axes,
                        targetPositions,
                        midPositions,
                        circle,
                        0,
                        mark
                    )
            );

            if (result != 0)
            {
                _logger.LogError("添加三点式圆弧段失败，错误码: {ErrorCode}", result);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "添加三点式圆弧段异常");
            return false;
        }
    }

    /// <summary>
    /// 设置圆弧限制参数
    /// </summary>
    public async Task<bool> SetArcLimitAsync(
        ushort crd,
        bool enable,
        double maxCenAcc,
        double maxArcError
    )
    {
        _logger.LogInformation(
            "设置坐标系 {Crd} 圆弧限制: Enable={Enable}, MaxCenAcc={MaxCenAcc}, MaxArcError={MaxArcError}",
            crd,
            enable,
            maxCenAcc,
            maxArcError
        );

        try
        {
            ushort enableFlag = (ushort)(enable ? 1 : 0);
            var result = await Task.Run(
                () => LTDMC.dmc_set_arc_limit(cardNo, crd, enableFlag, maxCenAcc, maxArcError)
            );

            if (result != 0)
            {
                _logger.LogError("设置圆弧限制失败，错误码: {ErrorCode}", result);
                return false;
            }
            _logger.LogDebug("圆弧限制设置成功");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "设置圆弧限制异常");
            return false;
        }
    }

    /// <summary>
    /// 获取圆弧限制参数
    /// </summary>
    public async Task<(bool enable, double maxCenAcc, double maxArcError)?> GetArcLimitAsync(
        ushort crd
    )
    {
        try
        {
            ushort enable = 0;
            double maxCenAcc = 0,
                maxArcError = 0;

            var result = await Task.Run(
                () =>
                    LTDMC.dmc_get_arc_limit(cardNo, crd, ref enable, ref maxCenAcc, ref maxArcError)
            );

            if (result != 0)
            {
                _logger.LogError("获取圆弧限制失败，错误码: {ErrorCode}", result);
                return null;
            }

            return (enable == 1, maxCenAcc, maxArcError);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取圆弧限制异常");
            return null;
        }
    }

    /// <summary>
    /// 计算圆心圆弧弧长
    /// </summary>
    public async Task<double?> CalculateArcLengthByCenterAsync(
        double[] startPos,
        double[] targetPos,
        double[] cenPos,
        bool clockwise,
        double circle = 0
    )
    {
        if (startPos == null || startPos.Length < 2)
            throw new ArgumentException("起始位置数组至少需要2个元素", nameof(startPos));

        if (targetPos == null || targetPos.Length != startPos.Length)
            throw new ArgumentException(
                "目标位置数组长度必须与起始位置数组相同",
                nameof(targetPos)
            );

        if (cenPos == null || cenPos.Length != startPos.Length)
            throw new ArgumentException("圆心位置数组长度必须与起始位置数组相同", nameof(cenPos));

        try
        {
            double arcLength = 0;
            ushort arcDir = (ushort)(clockwise ? 0 : 1);

            var result = await Task.Run(
                () =>
                    LTDMC.dmc_calculate_arclength_center(
                        startPos,
                        targetPos,
                        cenPos,
                        arcDir,
                        circle,
                        ref arcLength
                    )
            );

            if (result != 0)
            {
                _logger.LogError("计算圆心圆弧弧长失败，错误码: {ErrorCode}", result);
                return null;
            }

            return arcLength;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "计算圆心圆弧弧长异常");
            return null;
        }
    }

    /// <summary>
    /// 计算三点圆弧弧长
    /// </summary>
    public async Task<double?> CalculateArcLengthBy3PointsAsync(
        double[] startPos,
        double[] midPos,
        double[] targetPos,
        double circle = 0
    )
    {
        if (startPos == null || startPos.Length < 2)
            throw new ArgumentException("起始位置数组至少需要2个元素", nameof(startPos));

        if (midPos == null || midPos.Length != startPos.Length)
            throw new ArgumentException("中间点位置数组长度必须与起始位置数组相同", nameof(midPos));

        if (targetPos == null || targetPos.Length != startPos.Length)
            throw new ArgumentException(
                "目标位置数组长度必须与起始位置数组相同",
                nameof(targetPos)
            );

        try
        {
            double arcLength = 0;

            var result = await Task.Run(
                () =>
                    LTDMC.dmc_calculate_arclength_3point(
                        startPos,
                        midPos,
                        targetPos,
                        circle,
                        ref arcLength
                    )
            );

            if (result != 0)
            {
                _logger.LogError("计算三点圆弧弧长失败，错误码: {ErrorCode}", result);
                return null;
            }

            return arcLength;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "计算三点圆弧弧长异常");
            return null;
        }
    }

    /// <summary>
    /// 计算半径圆弧弧长
    /// </summary>
    public async Task<double?> CalculateArcLengthByRadiusAsync(
        double[] startPos,
        double[] targetPos,
        double arcRadius,
        bool clockwise,
        double circle = 0
    )
    {
        if (startPos == null || startPos.Length < 2)
            throw new ArgumentException("起始位置数组至少需要2个元素", nameof(startPos));

        if (targetPos == null || targetPos.Length != startPos.Length)
            throw new ArgumentException(
                "目标位置数组长度必须与起始位置数组相同",
                nameof(targetPos)
            );

        try
        {
            double arcLength = 0;
            ushort arcDir = (ushort)(clockwise ? 0 : 1);

            var result = await Task.Run(
                () =>
                    LTDMC.dmc_calculate_arclength_radius(
                        startPos,
                        targetPos,
                        arcRadius,
                        arcDir,
                        circle,
                        ref arcLength
                    )
            );

            if (result != 0)
            {
                _logger.LogError("计算半径圆弧弧长失败，错误码: {ErrorCode}", result);
                return null;
            }

            return arcLength;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "计算半径圆弧弧长异常");
            return null;
        }
    }

    /// <summary>
    /// 等待连续插补完成
    /// </summary>
    public async Task<bool> WaitContinuousCompleteAsync(
        ushort crd,
        int timeoutMs = 60000,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogInformation("等待坐标系 {Crd} 插补完成，超时时间: {Timeout}ms", crd, timeoutMs);

        try
        {
            var result = await AsyncHelper.PollWithBackoffAsync(
                () => CheckContinuousDoneAsync(crd),
                isDone => isDone,
                timeoutMs,
                100, // 初始延迟 100ms
                500, // 最大延迟 500ms
                cancellationToken
            );

            if (result)
            {
                _logger.LogInformation("坐标系 {Crd} 插补完成", crd);

                // 触发插补完成事件
                InterpolationCompleted?.Invoke(
                    this,
                    new InterpolationCompletedEventArgs { CoordinateSystem = crd, Success = true }
                );
            }

            return result;
        }
        catch (TimeoutException)
        {
            _logger.LogWarning("坐标系 {Crd} 插补等待超时", crd);
            return false;
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("坐标系 {Crd} 插补等待被取消", crd);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "等待坐标系 {Crd} 插补完成异常", crd);
            throw new MotionCardException($"等待插补完成异常", ex);
        }
    }

    /// <summary>
    /// 设置插补速度参数
    /// </summary>
    public async Task<bool> SetInterpolationParametersAsync(
        ushort crd,
        InterpolationParameters parameters
    )
    {
        ArgumentNullException.ThrowIfNull(parameters);
        parameters.Validate();

        _logger.LogInformation(
            "设置坐标系 {Crd} 插补参数: MaxSpeed={MaxSpeed}, Acc={Acc}, Dec={Dec}",
            crd,
            parameters.MaxSpeed,
            parameters.AccelerationTime,
            parameters.DecelerationTime
        );

        try
        {
            var result = await Task.Run(
                () =>
                    LTDMC.dmc_set_vector_profile_unit(
                        cardNo,
                        crd,
                        parameters.MinSpeed,
                        parameters.MaxSpeed,
                        parameters.AccelerationTime,
                        parameters.DecelerationTime,
                        parameters.StopSpeed
                    )
            );

            if (result != 0)
            {
                _logger.LogError("设置插补参数失败，错误码: {ErrorCode}", result);
                throw new MotionCardException($"设置插补参数失败", result);
            }

            // 缓存参数
            _parametersCache[crd] = parameters;
            _logger.LogDebug("坐标系 {Crd} 插补参数设置成功", crd);
            return true;
        }
        catch (MotionCardException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "设置坐标系 {Crd} 插补参数异常", crd);
            throw new MotionCardException($"设置插补参数异常", ex);
        }
    }

    /// <summary>
    /// 获取插补速度参数
    /// </summary>
    public async Task<InterpolationParameters?> GetInterpolationParametersAsync(ushort crd)
    {
        // 先检查缓存
        if (_parametersCache.TryGetValue(crd, out var cached))
        {
            return cached;
        }

        try
        {
            double minVel = 0,
                maxVel = 0,
                tacc = 0,
                tdec = 0,
                stopVel = 0;

            var result = await Task.Run(
                () =>
                    LTDMC.dmc_get_vector_profile_unit(
                        cardNo,
                        crd,
                        ref minVel,
                        ref maxVel,
                        ref tacc,
                        ref tdec,
                        ref stopVel
                    )
            );

            if (result != 0)
            {
                _logger.LogError("获取插补参数失败，错误码: {ErrorCode}", result);
                return null;
            }

            var parameters = new InterpolationParameters
            {
                MinSpeed = minVel,
                MaxSpeed = maxVel,
                AccelerationTime = tacc,
                DecelerationTime = tdec,
                StopSpeed = stopVel,
            };

            // 缓存参数
            _parametersCache[crd] = parameters;

            return parameters;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取坐标系 {Crd} 插补参数异常", crd);
            return null;
        }
    }
}
