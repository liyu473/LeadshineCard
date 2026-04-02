using LeadshineCard.Core.Exceptions;
using LeadshineCard.Core.Interfaces;
using LeadshineCard.Core.Models;
using LeadshineCard.ThirdPart;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace LeadshineCard.Implementation;

/// <summary>
/// 雷赛运动控制卡实现。
/// </summary>
/// <param name="logger">日志记录器</param>
/// <param name="loggerFactory">日志工厂</param>
public class LeadshineMotionCard(
    ILogger<LeadshineMotionCard>? logger = null,
    ILoggerFactory? loggerFactory = null
) : IMotionCard
{
    private const ushort MaxSupportedCards = 8;

    private readonly ILogger<LeadshineMotionCard> _logger =
        logger ?? NullLogger<LeadshineMotionCard>.Instance;
    private readonly ILoggerFactory _loggerFactory = loggerFactory ?? NullLoggerFactory.Instance;
    private readonly Dictionary<ushort, IAxisController> _axisControllers = [];
    private readonly object _connectionLock = new();
    private readonly SemaphoreSlim _lifecycleLock = new(1, 1);

    private ushort _cardNo;
    private bool _isConnected;
    private bool _disposed;
    private bool _ownsGlobalLifecycle = true;
    private CardInfo? _cardInfo;
    private IIoController? _ioController;
    private IInterpolationController? _interpolationController;
    private Timer? _heartbeatTimer;

    public ushort CardNo => _cardNo;
    public bool IsConnected => _isConnected;

    public event EventHandler<bool>? ConnectionStateChanged;

    public async Task<bool> InitializeAsync(ushort cardNo, bool heartbeat = true)
    {
        await _lifecycleLock.WaitAsync().ConfigureAwait(false);

        try
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            if (_isConnected)
            {
                if (_cardNo == cardNo)
                {
                    if (heartbeat && _heartbeatTimer is null)
                    {
                        StartHeartbeat();
                    }

                    return true;
                }

                throw new InvalidOperationException(
                    $"当前板卡实例已绑定到板卡 {_cardNo}，不能重新初始化为板卡 {cardNo}"
                );
            }

            _logger.LogInformation("开始初始化板卡 {CardNo}", cardNo);

            var detectedCardNos = await InitializeGlobalAndGetCardNosAsync().ConfigureAwait(false);
            if (!detectedCardNos.Contains(cardNo))
            {
                await CloseGlobalInitializationAsync().ConfigureAwait(false);
                throw new CardInitializationException($"未找到板卡 {cardNo}", 0);
            }

            _ownsGlobalLifecycle = true;
            await BindCardAsync(cardNo, heartbeat).ConfigureAwait(false);

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
        finally
        {
            _lifecycleLock.Release();
        }
    }

    public async Task<bool> CloseAsync()
    {
        await _lifecycleLock.WaitAsync().ConfigureAwait(false);

        try
        {
            if (!_isConnected)
            {
                return true;
            }

            if (!_ownsGlobalLifecycle)
            {
                DisconnectLocalState();
                return true;
            }

            _logger.LogInformation("关闭板卡 {CardNo}", _cardNo);

            DisconnectLocalState();

            var result = await Task.Run(() => LTDMC.dmc_board_close()).ConfigureAwait(false);
            if (result != 0)
            {
                _logger.LogError("关闭板卡失败，错误码: {ErrorCode}", result);
                return false;
            }

            _logger.LogInformation("板卡 {CardNo} 已关闭", _cardNo);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "板卡关闭异常");
            throw new MotionCardException("板卡关闭异常", ex);
        }
        finally
        {
            _lifecycleLock.Release();
        }
    }

    public async Task<bool> ResetAsync()
    {
        await _lifecycleLock.WaitAsync().ConfigureAwait(false);

        try
        {
            if (!_isConnected)
            {
                throw new InvalidOperationException("板卡未连接");
            }

            if (!_ownsGlobalLifecycle)
            {
                throw new InvalidOperationException("多板卡管理模式下不支持单卡 ResetAsync");
            }

            _logger.LogInformation("复位板卡 {CardNo}", _cardNo);

            DisconnectLocalState();

            var result = await Task.Run(() => LTDMC.dmc_board_reset()).ConfigureAwait(false);
            if (result != 0)
            {
                _logger.LogError("板卡复位失败，错误码: {ErrorCode}", result);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "板卡复位异常");
            throw new MotionCardException("板卡复位异常", ex);
        }
        finally
        {
            _lifecycleLock.Release();
        }
    }

    public CardInfo GetCardInfo()
    {
        if (_cardInfo == null)
        {
            throw new InvalidOperationException("板卡未初始化或信息尚未加载");
        }

        return _cardInfo;
    }

    public IAxisController GetAxisController(ushort axisNo)
    {
        if (!_isConnected)
        {
            throw new InvalidOperationException("板卡未连接");
        }

        if (!_axisControllers.TryGetValue(axisNo, out var controller))
        {
            controller = new LeadshineAxisController(
                _cardNo,
                axisNo,
                _loggerFactory.CreateLogger<LeadshineAxisController>()
            );
            _axisControllers[axisNo] = controller;
        }

        return controller;
    }

    public IIoController GetIoController()
    {
        if (!_isConnected)
        {
            throw new InvalidOperationException("板卡未连接");
        }

        if (_ioController == null)
        {
            _ioController = new LeadshineIoController(
                _cardNo,
                _loggerFactory.CreateLogger<LeadshineIoController>()
            );
        }

        return _ioController;
    }

    public IInterpolationController GetInterpolationController()
    {
        if (!_isConnected)
        {
            throw new InvalidOperationException("板卡未连接");
        }

        if (_interpolationController == null)
        {
            _interpolationController = new LeadshineInterpolationController(
                _cardNo,
                _loggerFactory.CreateLogger<LeadshineInterpolationController>()
            );
        }

        return _interpolationController;
    }

    public async Task<bool> EmergencyStopAsync()
    {
        _logger.LogWarning("执行紧急停止");

        try
        {
            var result = await Task.Run(() => LTDMC.dmc_emg_stop(_cardNo)).ConfigureAwait(false);
            if (result != 0)
            {
                _logger.LogError("紧急停止失败，错误码: {ErrorCode}", result);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "紧急停止异常");
            throw new MotionCardException("紧急停止异常", ex);
        }
    }

    internal async Task AttachInitializedCardAsync(ushort cardNo, bool heartbeat = true)
    {
        await _lifecycleLock.WaitAsync().ConfigureAwait(false);

        try
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            if (_isConnected)
            {
                if (_cardNo == cardNo)
                {
                    if (heartbeat && _heartbeatTimer is null)
                    {
                        StartHeartbeat();
                    }

                    return;
                }

                throw new InvalidOperationException(
                    $"当前板卡实例已绑定到板卡 {_cardNo}，不能重新绑定为板卡 {cardNo}"
                );
            }

            _ownsGlobalLifecycle = false;
            await BindCardAsync(cardNo, heartbeat).ConfigureAwait(false);
        }
        finally
        {
            _lifecycleLock.Release();
        }
    }

    private async Task BindCardAsync(ushort cardNo, bool heartbeat)
    {
        _cardNo = cardNo;
        _isConnected = true;

        await LoadCardInfoAsync().ConfigureAwait(false);

        if (heartbeat)
        {
            StartHeartbeat();
        }
    }

    private async Task<ushort[]> InitializeGlobalAndGetCardNosAsync()
    {
        var initResult = await Task.Run(() => LTDMC.dmc_board_init()).ConfigureAwait(false);

        if (initResult == 0)
        {
            throw new CardInitializationException("未检测到控制卡", 0);
        }

        if (initResult < 0)
        {
            var duplicatedCardNo = Math.Abs(initResult) - 1;
            throw new CardInitializationException(
                $"检测到重复硬件卡号 {duplicatedCardNo}",
                initResult
            );
        }

        ushort cardCount = 0;
        uint[] cardTypeList = new uint[MaxSupportedCards];
        ushort[] cardIdList = new ushort[MaxSupportedCards];

        var infoResult = await Task.Run(
                () => LTDMC.dmc_get_CardInfList(ref cardCount, cardTypeList, cardIdList)
            )
            .ConfigureAwait(false);

        if (infoResult != 0)
        {
            await CloseGlobalInitializationAsync().ConfigureAwait(false);
            throw new MotionCardException($"获取板卡信息列表失败，错误码: {infoResult}");
        }

        return cardIdList.Take(cardCount).ToArray();
    }

    private async Task CloseGlobalInitializationAsync()
    {
        var closeResult = await Task.Run(() => LTDMC.dmc_board_close()).ConfigureAwait(false);
        if (closeResult != 0)
        {
            _logger.LogWarning("释放全局板卡资源失败，错误码: {ErrorCode}", closeResult);
        }
    }

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

            uint cardVersion = 0;
            var result = await Task.Run(() => LTDMC.dmc_get_card_version(_cardNo, ref cardVersion))
                .ConfigureAwait(false);
            if (result == 0)
            {
                _cardInfo.CardVersion = cardVersion;
            }

            uint firmId = 0;
            uint subFirmId = 0;
            result = await Task.Run(
                    () => LTDMC.dmc_get_card_soft_version(_cardNo, ref firmId, ref subFirmId)
                )
                .ConfigureAwait(false);
            if (result == 0)
            {
                _cardInfo.FirmwareVersion = firmId;
                _cardInfo.SubFirmwareVersion = subFirmId;
            }

            uint libVer = 0;
            result = await Task.Run(() => LTDMC.dmc_get_card_lib_version(ref libVer))
                .ConfigureAwait(false);
            if (result == 0)
            {
                _cardInfo.LibraryVersion = libVer;
            }

            uint totalAxes = 0;
            result = await Task.Run(() => LTDMC.dmc_get_total_axes(_cardNo, ref totalAxes))
                .ConfigureAwait(false);
            if (result == 0)
            {
                _cardInfo.TotalAxes = totalAxes;
            }

            ushort totalIn = 0;
            ushort totalOut = 0;
            result = await Task.Run(
                    () => LTDMC.dmc_get_total_ionum(_cardNo, ref totalIn, ref totalOut)
                )
                .ConfigureAwait(false);
            if (result == 0)
            {
                _cardInfo.TotalInputs = totalIn;
                _cardInfo.TotalOutputs = totalOut;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "加载板卡信息失败");
        }
    }

    private void StartHeartbeat()
    {
        StopHeartbeat();
        _heartbeatTimer = new Timer(
            HeartbeatCallback,
            null,
            TimeSpan.FromSeconds(0.5),
            TimeSpan.FromSeconds(2)
        );
    }

    private void StopHeartbeat()
    {
        _heartbeatTimer?.Dispose();
        _heartbeatTimer = null;
    }

    private void DisconnectLocalState()
    {
        StopHeartbeat();
        _isConnected = false;
        _cardInfo = null;
        _ioController = null;
        _interpolationController = null;
        _axisControllers.Clear();
    }

    private void HeartbeatCallback(object? state)
    {
        lock (_connectionLock)
        {
            try
            {
                var result = LTDMC.dmc_check_done(_cardNo, 0);
                var isNowConnected = result == 0 || result == 1;
                if (isNowConnected != _isConnected)
                {
                    _isConnected = isNowConnected;
                    ConnectionStateChanged?.Invoke(this, _isConnected);
                }
            }
            catch (Exception ex)
            {
                if (_isConnected)
                {
                    _isConnected = false;
                    _logger.LogError(ex, "心跳检测异常，板卡可能已断开");
                    ConnectionStateChanged?.Invoke(this, false);
                }
            }
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

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
        _lifecycleLock.Dispose();
        GC.SuppressFinalize(this);
    }
}
