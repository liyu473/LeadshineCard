using LeadshineCard.Core.Exceptions;
using LeadshineCard.Core.Interfaces;
using LeadshineCard.Core.Models;
using LeadshineCard.ThirdPart;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace LeadshineCard.Implementation;

/// <summary>
/// 雷赛运动控制卡实现
/// </summary>
/// <remarks>
/// 构造函数
/// </remarks>
/// <param name="logger">日志记录器，为null时使用NullLogger</param>
/// <param name="loggerFactory">日志工厂，为null时使用NullLoggerFactory</param>
public class LeadshineMotionCard(
    ILogger<LeadshineMotionCard>? logger = null,
    ILoggerFactory? loggerFactory = null
) : IMotionCard
{
    private readonly ILogger<LeadshineMotionCard> _logger =
        logger ?? NullLogger<LeadshineMotionCard>.Instance;
    private readonly ILoggerFactory _loggerFactory = loggerFactory ?? NullLoggerFactory.Instance;
    private ushort _cardNo;
    private bool _isConnected;
    private bool _disposed;
    private CardInfo? _cardInfo;
    private readonly Dictionary<ushort, IAxisController> _axisControllers = [];
    private IIoController? _ioController;
    private IInterpolationController? _interpolationController;
    private Timer? _heartbeatTimer;
    private readonly object _connectionLock = new();
    private DateTime? _lastResetTime;

    public ushort CardNo => _cardNo;
    public bool IsConnected => _isConnected;

    /// <summary>
    /// 连接状态变更事件
    /// </summary>
    public event EventHandler<bool>? ConnectionStateChanged;

    /// <summary>
    /// 初始化板卡
    /// </summary>
    public async Task<bool> InitializeAsync(ushort cardNo, bool heartbeat = true)
    {
        _logger.LogInformation("开始初始化板卡 {CardNo}", cardNo);

        try
        {
            if (_lastResetTime.HasValue)
            {
                var elapsed = DateTime.UtcNow - _lastResetTime.Value;
                var waitTime = TimeSpan.FromSeconds(5) - elapsed;
                if (waitTime > TimeSpan.Zero)
                {
                    _logger.LogInformation(
                        "根据 DMC3000 文档要求，复位后初始化前需等待 5 秒，等待 {WaitMs}ms",
                        (int)waitTime.TotalMilliseconds
                    );
                    await Task.Delay(waitTime);
                }
            }

            // 异步执行初始化
            var cardCount = await Task.Run(() => LTDMC.dmc_board_init());

            // 文档: 0=无卡/异常, 1~8=卡数量, 负值=存在重复卡号
            if (cardCount == 0)
            {
                _logger.LogError("板卡初始化失败：未找到控制卡或控制卡异常");
                throw new CardInitializationException("板卡初始化失败：未找到控制卡或控制卡异常", 0);
            }

            if (cardCount < 0)
            {
                var duplicatedCardNo = (short)(Math.Abs(cardCount) - 1);
                _logger.LogError("板卡初始化失败：检测到重复硬件卡号 {CardNo}", duplicatedCardNo);
                throw new CardInitializationException(
                    $"板卡初始化失败：检测到重复硬件卡号 {duplicatedCardNo}",
                    cardCount
                );
            }

            if (cardNo >= cardCount)
            {
                _logger.LogError(
                    "请求初始化卡号 {CardNo} 超出范围，当前仅检测到 {CardCount} 张卡（有效卡号 0~{MaxCardNo}）",
                    cardNo,
                    cardCount,
                    cardCount - 1
                );
                _ = await Task.Run(() => LTDMC.dmc_board_close());
                throw new CardInitializationException(
                    $"卡号 {cardNo} 超出范围，当前仅检测到 {cardCount} 张卡",
                    -1
                );
            }

            _cardNo = cardNo;
            _isConnected = true;

            // 查询板卡信息
            await LoadCardInfoAsync();

            // 启动心跳检测
            if (heartbeat)
                StartHeartbeat();

            _logger.LogInformation("板卡 {CardNo} 初始化成功", cardNo);
            return true;
        }
        catch (CardInitializationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "板卡初始化异常");
            throw new CardInitializationException("板卡初始化异常", ex);
        }
    }

    /// <summary>
    /// 关闭板卡
    /// </summary>
    public async Task<bool> CloseAsync()
    {
        if (!_isConnected)
        {
            _logger.LogWarning("板卡未连接，无需关闭");
            return true;
        }
        _logger.LogInformation("关闭板卡 {CardNo}", _cardNo);

        try
        {
            var result = await Task.Run(() => LTDMC.dmc_board_close());

            if (result != 0)
            {
                _logger.LogError("板卡关闭失败，错误码: {ErrorCode}", result);
                return false;
            }

            _isConnected = false;
            _axisControllers.Clear();
            _ioController = null;
            _interpolationController = null;

            // 停止心跳检测
            StopHeartbeat();

            _logger.LogInformation("板卡 {CardNo} 已关闭", _cardNo);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "板卡关闭异常");
            throw new MotionCardException("板卡关闭异常", ex);
        }
    }

    /// <summary>
    /// 复位板卡
    /// </summary>
    public async Task<bool> ResetAsync()
    {
        _logger.LogInformation("复位板卡 {CardNo}", _cardNo);

        try
        {
            var result = await Task.Run(() => LTDMC.dmc_board_reset());

            if (result != 0)
            {
                _logger.LogError("板卡复位失败，错误码: {ErrorCode}", result);
                return false;
            }
            _lastResetTime = DateTime.UtcNow;
            _logger.LogInformation("板卡复位完成，按文档要求应等待 5 秒后再初始化");
            _logger.LogInformation("板卡 {CardNo} 复位成功", _cardNo);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "板卡复位异常");
            throw new MotionCardException("板卡复位异常", ex);
        }
    }

    /// <summary>
    /// 获取板卡信息
    /// </summary>
    public CardInfo GetCardInfo()
    {
        if (_cardInfo == null)
        {
            throw new InvalidOperationException("板卡未初始化或信息未加载");
        }

        return _cardInfo;
    }

    /// <summary>
    /// 获取轴控制器
    /// </summary>
    public IAxisController GetAxisController(ushort axisNo)
    {
        if (!_isConnected)
        {
            throw new InvalidOperationException("板卡未连接");
        }

        if (!_axisControllers.TryGetValue(axisNo, out var controller))
        {
            _logger.LogDebug("创建轴 {AxisNo} 控制器", axisNo);
            controller = new LeadshineAxisController(
                _cardNo,
                axisNo,
                _loggerFactory.CreateLogger<LeadshineAxisController>()
            );
            _axisControllers[axisNo] = controller;
        }

        return controller;
    }

    /// <summary>
    /// 获取IO控制器
    /// </summary>
    public IIoController GetIoController()
    {
        if (!_isConnected)
        {
            throw new InvalidOperationException("板卡未连接");
        }

        if (_ioController == null)
        {
            _logger.LogDebug("创建IO控制器");
            _ioController = new LeadshineIoController(
                _cardNo,
                _loggerFactory.CreateLogger<LeadshineIoController>()
            );
        }

        return _ioController;
    }

    /// <summary>
    /// 获取插补控制器
    /// </summary>
    public IInterpolationController GetInterpolationController()
    {
        if (!_isConnected)
        {
            throw new InvalidOperationException("板卡未连接");
        }

        if (_interpolationController == null)
        {
            _logger.LogDebug("创建插补控制器");
            _interpolationController = new LeadshineInterpolationController(
                _cardNo,
                _loggerFactory.CreateLogger<LeadshineInterpolationController>()
            );
        }

        return _interpolationController;
    }

    /// <summary>
    /// 紧急停止所有轴
    /// </summary>
    public async Task<bool> EmergencyStopAsync()
    {
        _logger.LogWarning("执行紧急停止");

        try
        {
            var result = await Task.Run(() => LTDMC.dmc_emg_stop(_cardNo));

            if (result != 0)
            {
                _logger.LogError("紧急停止失败，错误码: {ErrorCode}", result);
                return false;
            }
            _logger.LogInformation("紧急停止执行成功");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "紧急停止异常");
            throw new MotionCardException("紧急停止异常", ex);
        }
    }

    /// <summary>
    /// 加载板卡信息
    /// </summary>
    private async Task LoadCardInfoAsync()
    {
        try
        {
            _cardInfo = new CardInfo
            {
                CardNo = _cardNo,
                IsConnected = true,
                InitializedTime = DateTime.Now,
            };

            // 获取板卡版本
            uint cardVersion = 0;
            var result = await Task.Run(() => LTDMC.dmc_get_card_version(_cardNo, ref cardVersion));
            if (result == 0)
            {
                _cardInfo.CardVersion = cardVersion;
            }

            // 获取固件版本
            uint firmId = 0,
                subFirmId = 0;
            result = await Task.Run(
                () => LTDMC.dmc_get_card_soft_version(_cardNo, ref firmId, ref subFirmId)
            );
            if (result == 0)
            {
                _cardInfo.FirmwareVersion = firmId;
                _cardInfo.SubFirmwareVersion = subFirmId;
            }

            // 获取库版本
            uint libVer = 0;
            result = await Task.Run(() => LTDMC.dmc_get_card_lib_version(ref libVer));
            if (result == 0)
            {
                _cardInfo.LibraryVersion = libVer;
            }

            // 获取总轴数
            uint totalAxes = 0;
            result = await Task.Run(() => LTDMC.dmc_get_total_axes(_cardNo, ref totalAxes));
            if (result == 0)
            {
                _cardInfo.TotalAxes = totalAxes;
            }

            // 获取IO数量
            ushort totalIn = 0,
                totalOut = 0;
            result = await Task.Run(
                () => LTDMC.dmc_get_total_ionum(_cardNo, ref totalIn, ref totalOut)
            );
            if (result == 0)
            {
                _cardInfo.TotalInputs = totalIn;
                _cardInfo.TotalOutputs = totalOut;
            }
            _logger.LogInformation("板卡信息: {CardInfo}", _cardInfo);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "加载板卡信息失败");
        }
    }

    /// <summary>
    /// 启动心跳检测
    /// </summary>
    private void StartHeartbeat()
    {
        _heartbeatTimer = new Timer(
            HeartbeatCallback,
            null,
            TimeSpan.FromSeconds(0.5),
            TimeSpan.FromSeconds(2)
        );
        _logger.LogDebug("心跳检测已启动");
    }

    /// <summary>
    /// 停止心跳检测
    /// </summary>
    private void StopHeartbeat()
    {
        _heartbeatTimer?.Dispose();
        _heartbeatTimer = null;
        _logger.LogDebug("心跳检测已停止");
    }

    /// <summary>
    /// 心跳检测回调
    /// </summary>
    private void HeartbeatCallback(object? state)
    {
        lock (_connectionLock)
        {
            try
            {
                // 使用轻量级API检测板卡状态
                var result = LTDMC.dmc_check_done(_cardNo, 0);
                bool isNowConnected = result == 0 || result == 1; // 0=运动中, 1=运动完成, 都表示连接正常

                if (isNowConnected != _isConnected)
                {
                    _isConnected = isNowConnected;
                    _logger.LogWarning("板卡连接状态变更: {IsConnected}", _isConnected);

                    // 触发事件
                    ConnectionStateChanged?.Invoke(this, _isConnected);
                }
            }
            catch (Exception ex)
            {
                if (_isConnected)
                {
                    _isConnected = false;
                    _logger.LogError(ex, "心跳检测异常,板卡可能已断开");
                    ConnectionStateChanged?.Invoke(this, false);
                }
            }
        }
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;
        _logger.LogDebug("释放板卡资源");

        // 停止心跳检测
        StopHeartbeat();

        if (_isConnected)
        {
            try
            {
                CloseAsync().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "释放资源时关闭板卡失败");
            }
        }

        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
