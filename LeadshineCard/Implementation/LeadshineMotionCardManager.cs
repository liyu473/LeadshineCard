using System.Collections.Concurrent;
using LeadshineCard.Core.Exceptions;
using LeadshineCard.Core.Interfaces;
using LeadshineCard.ThirdPart;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace LeadshineCard.Implementation;

/// <summary>
/// 多板卡管理器。
/// </summary>
/// <param name="logger">日志记录器</param>
/// <param name="loggerFactory">日志工厂</param>
public class LeadshineMotionCardManager(
    ILogger<LeadshineMotionCardManager>? logger = null,
    ILoggerFactory? loggerFactory = null
) : IMotionCardManager, IAsyncDisposable
{
    private const ushort MaxSupportedCards = 8;

    private readonly ILogger<LeadshineMotionCardManager> _logger =
        logger ?? NullLogger<LeadshineMotionCardManager>.Instance;
    private readonly ILoggerFactory _loggerFactory = loggerFactory ?? NullLoggerFactory.Instance;
    private readonly ConcurrentDictionary<ushort, Lazy<Task<LeadshineMotionCard>>> _cards = new();
    private readonly SemaphoreSlim _operationLock = new(1, 1);

    private ushort[]? _detectedCardNos;
    private bool _isGloballyInitialized;
    private bool _disposed;

    public async Task<ushort> GetDetectedCardCountAsync()
    {
        var cardNos = await GetDetectedCardNosAsync().ConfigureAwait(false);
        return checked((ushort)cardNos.Count);
    }

    public async Task<IReadOnlyList<ushort>> GetDetectedCardNosAsync()
    {
        await EnsureGlobalInitializedAsync().ConfigureAwait(false);
        return _detectedCardNos ?? [];
    }

    public async Task<IMotionCard> GetCardAsync(ushort cardNo, bool heartbeat = true)
    {
        ThrowIfDisposed();

        var detectedCardNos = await GetDetectedCardNosAsync().ConfigureAwait(false);
        if (!detectedCardNos.Contains(cardNo))
        {
            throw new CardInitializationException($"未检测到板卡 {cardNo}", 0);
        }

        var lazyCard = _cards.GetOrAdd(
            cardNo,
            key =>
                new Lazy<Task<LeadshineMotionCard>>(
                    () => CreateAndAttachCardAsync(key, heartbeat),
                    LazyThreadSafetyMode.ExecutionAndPublication
                )
        );

        try
        {
            return await lazyCard.Value.ConfigureAwait(false);
        }
        catch
        {
            _cards.TryRemove(new KeyValuePair<ushort, Lazy<Task<LeadshineMotionCard>>>(cardNo, lazyCard));
            throw;
        }
    }

    public async Task InitializeCardsAsync(IEnumerable<ushort> cardNos, bool heartbeat = true)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(cardNos);

        var distinctCardNos = cardNos.Distinct().ToArray();
        foreach (var cardNo in distinctCardNos)
        {
            await GetCardAsync(cardNo, heartbeat).ConfigureAwait(false);
        }
    }

    public async Task InitializeAllCardsAsync(bool heartbeat = true)
    {
        var cardNos = await GetDetectedCardNosAsync().ConfigureAwait(false);
        if (cardNos.Count == 0)
        {
            throw new CardInitializationException("未检测到控制卡", 0);
        }

        await InitializeCardsAsync(cardNos, heartbeat).ConfigureAwait(false);
    }

    public IReadOnlyCollection<IMotionCard> GetInitializedCards()
    {
        ThrowIfDisposed();

        return
        [
            .. _cards
                .Values
                .Where(lazyCard => lazyCard.IsValueCreated && lazyCard.Value.IsCompletedSuccessfully)
                .Select(lazyCard => (IMotionCard)lazyCard.Value.Result),
        ];
    }

    public async Task<bool> CloseCardAsync(ushort cardNo)
    {
        ThrowIfDisposed();

        if (!_cards.TryRemove(cardNo, out var lazyCard))
        {
            return true;
        }

        try
        {
            var card = await lazyCard.Value.ConfigureAwait(false);
            return !card.IsConnected || await card.CloseAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "关闭板卡 {CardNo} 时发生异常", cardNo);
            return false;
        }
    }

    public async Task CloseAllAsync()
    {
        if (_disposed)
        {
            return;
        }

        var cardNos = _cards.Keys.ToArray();
        foreach (var cardNo in cardNos)
        {
            await CloseCardAsync(cardNo).ConfigureAwait(false);
        }

        if (!_isGloballyInitialized)
        {
            return;
        }

        var closeResult = await Task.Run(() => LTDMC.dmc_board_close()).ConfigureAwait(false);
        if (closeResult != 0)
        {
            throw new MotionCardException($"关闭全局板卡资源失败，错误码: {closeResult}");
        }

        _isGloballyInitialized = false;
        _detectedCardNos = null;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        try
        {
            CloseAllAsync().GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "释放板卡管理器时关闭板卡失败");
        }

        _disposed = true;
        _operationLock.Dispose();
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        await CloseAllAsync().ConfigureAwait(false);
        _disposed = true;
        _operationLock.Dispose();
        GC.SuppressFinalize(this);
    }

    private async Task EnsureGlobalInitializedAsync()
    {
        ThrowIfDisposed();

        if (_isGloballyInitialized)
        {
            return;
        }

        await _operationLock.WaitAsync().ConfigureAwait(false);
        try
        {
            if (_isGloballyInitialized)
            {
                return;
            }

            var initResult = await Task.Run(() => LTDMC.dmc_board_init()).ConfigureAwait(false);
            if (initResult == 0)
            {
                _detectedCardNos = [];
                return;
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
                await CloseGlobalIfNeededAsync().ConfigureAwait(false);
                throw new MotionCardException($"获取板卡信息列表失败，错误码: {infoResult}");
            }

            _detectedCardNos = cardIdList.Take(cardCount).ToArray();
            _isGloballyInitialized = true;
        }
        finally
        {
            _operationLock.Release();
        }
    }

    private async Task<LeadshineMotionCard> CreateAndAttachCardAsync(ushort cardNo, bool heartbeat)
    {
        var card = new LeadshineMotionCard(
            _loggerFactory.CreateLogger<LeadshineMotionCard>(),
            _loggerFactory
        );

        try
        {
            await card.AttachInitializedCardAsync(cardNo, heartbeat).ConfigureAwait(false);
            return card;
        }
        catch
        {
            card.Dispose();
            throw;
        }
    }

    private async Task CloseGlobalIfNeededAsync()
    {
        var closeResult = await Task.Run(() => LTDMC.dmc_board_close()).ConfigureAwait(false);
        if (closeResult != 0)
        {
            _logger.LogWarning("释放全局板卡资源失败，错误码: {ErrorCode}", closeResult);
        }

        _isGloballyInitialized = false;
        _detectedCardNos = null;
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }
}
