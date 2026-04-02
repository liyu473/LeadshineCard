using LeadshineCard.Core.Enums;
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
public class LeadshineMotionCard : IMotionCard
{
    private readonly ILogger<LeadshineMotionCard> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly Dictionary<ushort, IAxisController> _axisControllers = [];
    private readonly object _lifecycleLock = new();
    private readonly bool _ownsGlobalLifecycle;

    private bool _disposed;
    private CardInfo? _cardInfo;
    private IIoController? _ioController;
    private IInterpolationController? _interpolationController;

    public LeadshineMotionCard(
        ushort cardNo,
        ILogger<LeadshineMotionCard>? logger = null,
        ILoggerFactory? loggerFactory = null
    )
        : this(cardNo, ownsGlobalLifecycle: true, logger, loggerFactory) { }

    internal LeadshineMotionCard(
        ushort cardNo,
        bool ownsGlobalLifecycle,
        ILogger<LeadshineMotionCard>? logger = null,
        ILoggerFactory? loggerFactory = null
    )
    {
        _logger = logger ?? NullLogger<LeadshineMotionCard>.Instance;
        _loggerFactory = loggerFactory ?? NullLoggerFactory.Instance;
        _ownsGlobalLifecycle = ownsGlobalLifecycle;
        CardNo = cardNo;
    }

    public ushort CardNo { get; }

    public bool IsConnected => _cardInfo != null;

    public Task<bool> InitializeAsync()
    {
        lock (_lifecycleLock)
        {
            if (_cardInfo != null)
            {
                _logger.LogWarning("板卡 {CardNo} 已经初始化", CardNo);
                return Task.FromResult(true);
            }

            if (_ownsGlobalLifecycle)
            {
                var detectedCardNos = LeadshineCardLibraryHelper.InitializeAndGetDetectedCardNos(_logger);
                if (!detectedCardNos.Contains(CardNo))
                {
                    LeadshineCardLibraryHelper.TryClose(_logger);
                    throw new CardInitializationException($"未检测到板卡 {CardNo}", 0);
                }
            }

            LoadCardInfo();
            _logger.LogInformation("板卡 {CardNo} 初始化成功", CardNo);
            return Task.FromResult(true);
        }
    }

    public Task<bool> CloseAsync()
    {
        lock (_lifecycleLock)
        {
            if (_cardInfo == null)
            {
                return Task.FromResult(true);
            }

            if (_ownsGlobalLifecycle)
            {
                try
                {
                    LeadshineCardLibraryHelper.Close();
                }
                catch (MotionCardException ex)
                {
                    _logger.LogError(ex, "关闭板卡失败");
                    return Task.FromResult(false);
                }
            }

            ClearLocalState();
            return Task.FromResult(true);
        }
    }

    public Task<bool> ResetAsync()
    {
        lock (_lifecycleLock)
        {
            if (_cardInfo == null)
            {
                throw new InvalidOperationException("板卡未连接");
            }

            if (!_ownsGlobalLifecycle)
            {
                throw new InvalidOperationException("多板卡管理模式下不支持单卡 ResetAsync");
            }

            try
            {
                LeadshineCardLibraryHelper.Reset();
            }
            catch (MotionCardException ex)
            {
                _logger.LogError(ex, "板卡复位失败");
                return Task.FromResult(false);
            }

            Thread.Sleep(TimeSpan.FromSeconds(5));

            try
            {
                var detectedCardNos = LeadshineCardLibraryHelper.InitializeAndGetDetectedCardNos(_logger);
                if (!detectedCardNos.Contains(CardNo))
                {
                    LeadshineCardLibraryHelper.TryClose(_logger);
                    ClearLocalState();
                    throw new CardInitializationException($"复位后未检测到板卡 {CardNo}", 0);
                }

                ClearLocalState();
                LoadCardInfo();
                return Task.FromResult(true);
            }
            catch (Exception ex) when (ex is not CardInitializationException)
            {
                ClearLocalState();
                _logger.LogError(ex, "板卡复位后重新初始化失败");
                throw new MotionCardException("板卡复位后重新初始化失败", ex);
            }
        }
    }

    public Task<bool> SetDebugModeAsync(DebugOutputMode mode, string fileName) =>
        LeadshineDebugModeHelper.SetDebugModeAsync(mode, fileName, _logger);

    public Task<DebugModeSettings> GetDebugModeAsync() =>
        LeadshineDebugModeHelper.GetDebugModeAsync(_logger);

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
        if (_cardInfo == null)
        {
            throw new InvalidOperationException("板卡未初始化，请先调用 InitializeAsync");
        }

        if (_cardInfo.TotalAxes > 0 && axisNo >= _cardInfo.TotalAxes)
        {
            throw new ArgumentOutOfRangeException(
                nameof(axisNo),
                $"轴号 {axisNo} 超出范围，板卡 {CardNo} 支持的轴数为 {_cardInfo.TotalAxes}"
            );
        }

        if (!_axisControllers.TryGetValue(axisNo, out var controller))
        {
            controller = new LeadshineAxisController(
                CardNo,
                axisNo,
                _loggerFactory.CreateLogger<LeadshineAxisController>()
            );
            _axisControllers[axisNo] = controller;
        }

        return controller;
    }

    public IIoController GetIoController()
    {
        if (_cardInfo == null)
        {
            throw new InvalidOperationException("板卡未初始化，请先调用 InitializeAsync");
        }

        if (_ioController == null)
        {
            _ioController = new LeadshineIoController(
                CardNo,
                _loggerFactory.CreateLogger<LeadshineIoController>()
            );
        }

        return _ioController;
    }

    public IInterpolationController GetInterpolationController()
    {
        if (_cardInfo == null)
        {
            throw new InvalidOperationException("板卡未初始化，请先调用 InitializeAsync");
        }

        if (_interpolationController == null)
        {
            _interpolationController = new LeadshineInterpolationController(
                CardNo,
                _loggerFactory.CreateLogger<LeadshineInterpolationController>()
            );
        }

        return _interpolationController;
    }

    public Task<bool> EmergencyStopAsync()
    {
        var result = LTDMC.dmc_emg_stop(CardNo);
        if (result != 0)
        {
            _logger.LogError("紧急停止失败，错误码: {ErrorCode}", result);
            return Task.FromResult(false);
        }

        return Task.FromResult(true);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        try
        {
            if (_cardInfo != null)
            {
                CloseAsync().GetAwaiter().GetResult();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "释放资源时关闭板卡失败");
        }

        _disposed = true;
        GC.SuppressFinalize(this);
    }

    private void LoadCardInfo()
    {
        try
        {
            _cardInfo = new CardInfo
            {
                CardNo = CardNo,
                CardId = CardNo,
                IsConnected = true,
                InitializedTime = DateTime.Now,
            };

            uint cardVersion = 0;
            var result = LTDMC.dmc_get_card_version(CardNo, ref cardVersion);
            if (result == 0)
            {
                _cardInfo.CardVersion = cardVersion;
            }

            uint firmId = 0;
            uint subFirmId = 0;
            result = LTDMC.dmc_get_card_soft_version(CardNo, ref firmId, ref subFirmId);
            if (result == 0)
            {
                _cardInfo.FirmwareVersion = firmId;
                _cardInfo.SubFirmwareVersion = subFirmId;
            }

            uint libVer = 0;
            result = LTDMC.dmc_get_card_lib_version(ref libVer);
            if (result == 0)
            {
                _cardInfo.LibraryVersion = libVer;
            }

            uint totalAxes = 0;
            result = LTDMC.dmc_get_total_axes(CardNo, ref totalAxes);
            if (result == 0)
            {
                _cardInfo.TotalAxes = totalAxes;
            }

            ushort totalIn = 0;
            ushort totalOut = 0;
            result = LTDMC.dmc_get_total_ionum(CardNo, ref totalIn, ref totalOut);
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

    private void ClearLocalState()
    {
        _cardInfo = null;
        _ioController = null;
        _interpolationController = null;
        _axisControllers.Clear();
    }
}
