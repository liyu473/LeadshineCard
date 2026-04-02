using System.Collections.Concurrent;
using LeadshineCard.Core.Enums;
using LeadshineCard.Core.Exceptions;
using LeadshineCard.Core.Interfaces;
using LeadshineCard.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace LeadshineCard.Implementation;

/// <summary>
/// 多板卡管理器。
/// </summary>
public class LeadshineMotionCardManager : IMotionCardManager, IAsyncDisposable
{
    private readonly ILogger<LeadshineMotionCardManager> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ConcurrentDictionary<ushort, IMotionCard> _cards = new();
    private readonly SemaphoreSlim _lifecycleLock = new(1, 1);
    private bool _disposed;
    private bool _isInitialized;

    public LeadshineMotionCardManager(
        ILogger<LeadshineMotionCardManager>? logger = null,
        ILoggerFactory? loggerFactory = null
    )
    {
        _logger = logger ?? NullLogger<LeadshineMotionCardManager>.Instance;
        _loggerFactory = loggerFactory ?? NullLoggerFactory.Instance;
    }

    public Task<ushort> GetDetectedCardCountAsync()
    {
        ThrowIfDisposed();
        return Task.FromResult(checked((ushort)_cards.Count));
    }

    public Task<IReadOnlyList<ushort>> GetDetectedCardNosAsync()
    {
        ThrowIfDisposed();
        IReadOnlyList<ushort> cardNos = [.. _cards.Keys.OrderBy(cardNo => cardNo)];
        return Task.FromResult(cardNos);
    }

    public Task<IMotionCard> GetCardAsync(ushort cardNo)
    {
        ThrowIfDisposed();

        if (!_isInitialized)
        {
            throw new InvalidOperationException("请先调用 InitializeAllCardsAsync");
        }

        if (!_cards.TryGetValue(cardNo, out var card))
        {
            throw new KeyNotFoundException($"未找到已初始化的板卡 {cardNo}");
        }

        return Task.FromResult(card);
    }

    public async Task InitializeAllCardsAsync()
    {
        ThrowIfDisposed();

        await _lifecycleLock.WaitAsync().ConfigureAwait(false);
        try
        {
            if (_isInitialized)
            {
                return;
            }

            var detectedCardNos = LeadshineCardLibraryHelper.InitializeAndGetDetectedCardNos(_logger);

            try
            {
                foreach (var cardNo in detectedCardNos)
                {
                    var card = new LeadshineMotionCard(
                        cardNo,
                        ownsGlobalLifecycle: false,
                        logger: _loggerFactory.CreateLogger<LeadshineMotionCard>(),
                        loggerFactory: _loggerFactory
                    );
                    _cards[cardNo] = card;
                }

                _isInitialized = true;
            }
            catch
            {
                foreach (var card in _cards.Values)
                {
                    card.Dispose();
                }

                _cards.Clear();
                LeadshineCardLibraryHelper.TryClose(_logger);
                throw;
            }
        }
        finally
        {
            _lifecycleLock.Release();
        }
    }

    public IReadOnlyCollection<IMotionCard> GetInitializedCards()
    {
        ThrowIfDisposed();
        return [.. _cards.OrderBy(pair => pair.Key).Select(pair => pair.Value)];
    }

    public async Task<bool> CloseCardAsync(ushort cardNo)
    {
        ThrowIfDisposed();

        if (!_cards.TryRemove(cardNo, out var card))
        {
            return true;
        }

        var closed = await card.CloseAsync().ConfigureAwait(false);

        if (_cards.IsEmpty && _isInitialized)
        {
            try
            {
                LeadshineCardLibraryHelper.Close();
                _isInitialized = false;
            }
            catch (MotionCardException ex)
            {
                _logger.LogError(ex, "关闭全局板卡资源失败");
                return false;
            }
        }

        return closed;
    }

    public async Task CloseAllAsync()
    {
        if (_disposed)
        {
            return;
        }

        await _lifecycleLock.WaitAsync().ConfigureAwait(false);
        try
        {
            var cards = _cards.OrderBy(pair => pair.Key).ToArray();
            foreach (var (_, card) in cards)
            {
                await card.CloseAsync().ConfigureAwait(false);
            }

            _cards.Clear();

            if (_isInitialized)
            {
                LeadshineCardLibraryHelper.Close();
                _isInitialized = false;
            }
        }
        finally
        {
            _lifecycleLock.Release();
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
        _lifecycleLock.Dispose();
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
        _lifecycleLock.Dispose();
        GC.SuppressFinalize(this);
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }
}
