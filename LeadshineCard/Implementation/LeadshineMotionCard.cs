using LeadshineCard.Core.Exceptions;
using LeadshineCard.Core.Interfaces;
using LeadshineCard.Core.Models;
using LeadshineCard.ThirdPart;
using Microsoft.Extensions.Logging;

namespace LeadshineCard.Implementation;

/// <summary>
/// 雷赛运动控制卡实现
/// </summary>
public class LeadshineMotionCard : IMotionCard
{
    private readonly ILogger<LeadshineMotionCard> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private ushort _cardNo;
    private bool _isConnected;
    private bool _disposed;
    private CardInfo? _cardInfo;
    private readonly Dictionary<ushort, IAxisController> _axisControllers;
    private IIoController? _ioController;
    private IInterpolationController? _interpolationController;

    public ushort CardNo => _cardNo;
    public bool IsConnected => _isConnected;

    /// <summary>
    /// 构造函数
    /// </summary>
    public LeadshineMotionCard(
        ILogger<LeadshineMotionCard> logger,
        ILoggerFactory loggerFactory)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        _axisControllers = new Dictionary<ushort, IAxisController>();
    }

    /// <summary>
    /// 初始化板卡
    /// </summary>
    public async Task<bool> InitializeAsync(ushort cardNo)
    {
        _logger.LogInformation("开始初始化板卡 {CardNo}", cardNo);

        try
        {
            // 异步执行初始化
            var result = await Task.Run(() => LTDMC.dmc_board_init());

            if (result != 0)
            {
                _logger.LogError("板卡初始化失败，错误码: {ErrorCode}", result);
                throw new CardInitializationException($"板卡初始化失败，错误码: {result}", result);
            }

            _cardNo = cardNo;
            _isConnected = true;

            // 查询板卡信息
            await LoadCardInfoAsync();

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
                _loggerFactory.CreateLogger<LeadshineAxisController>());
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
                _loggerFactory.CreateLogger<LeadshineIoController>());
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
                _loggerFactory.CreateLogger<LeadshineInterpolationController>());
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
                InitializedTime = DateTime.Now
            };

            // 获取板卡版本
            uint cardVersion = 0;
            var result = await Task.Run(() => LTDMC.dmc_get_card_version(_cardNo, ref cardVersion));
            if (result == 0)
            {
                _cardInfo.CardVersion = cardVersion;
            }

            // 获取固件版本
            uint firmId = 0, subFirmId = 0;
            result = await Task.Run(() => LTDMC.dmc_get_card_soft_version(_cardNo, ref firmId, ref subFirmId));
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
            ushort totalIn = 0, totalOut = 0;
            result = await Task.Run(() => LTDMC.dmc_get_total_ionum(_cardNo, ref totalIn, ref totalOut));
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
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;

        _logger.LogDebug("释放板卡资源");

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
