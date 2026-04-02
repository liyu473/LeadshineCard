using System.Collections.Concurrent;
using LeadshineCard.Core.Interfaces;
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
    private readonly ILogger<LeadshineMotionCardManager> _logger =
        logger ?? NullLogger<LeadshineMotionCardManager>.Instance;
    private readonly ILoggerFactory _loggerFactory = loggerFactory ?? NullLoggerFactory.Instance;
    private readonly ConcurrentDictionary<ushort, Lazy<Task<LeadshineMotionCard>>> _cards = new();
    private bool _disposed;

    public async Task<IMotionCard> GetCardAsync(ushort cardNo, bool heartbeat = true)
    {
        ThrowIfDisposed();

        var lazyCard = _cards.GetOrAdd(
            cardNo,
            key =>
                new Lazy<Task<LeadshineMotionCard>>(
                    () => CreateAndInitializeCardAsync(key, heartbeat),
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
        await Task.WhenAll(distinctCardNos.Select(cardNo => GetCardAsync(cardNo, heartbeat)))
            .ConfigureAwait(false);
    }

    public IReadOnlyCollection<IMotionCard> GetInitializedCards()
    {
        ThrowIfDisposed();

        return [.. _cards
            .Values.Where(lazyCard => lazyCard.IsValueCreated && lazyCard.Value.IsCompletedSuccessfully)
            .Select(lazyCard => (IMotionCard)lazyCard.Value.Result)];
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
        GC.SuppressFinalize(this);
    }

    private async Task<LeadshineMotionCard> CreateAndInitializeCardAsync(
        ushort cardNo,
        bool heartbeat
    )
    {
        var card = new LeadshineMotionCard(
            _loggerFactory.CreateLogger<LeadshineMotionCard>(),
            _loggerFactory
        );

        try
        {
            await card.InitializeAsync(cardNo, heartbeat).ConfigureAwait(false);
            _logger.LogInformation("板卡 {CardNo} 已完成初始化并加入管理器", cardNo);
            return card;
        }
        catch
        {
            card.Dispose();
            throw;
        }
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }
}
