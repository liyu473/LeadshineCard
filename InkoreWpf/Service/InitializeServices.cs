using LeadshineCard.Core.Enums;
using LeadshineCard.Core.Interfaces;
using LeadshineCard.Implementation;
using LyuExtensions.Aspects;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ZLogger;

namespace InkoreWpf.Service;

[HostedService]
public class InitializeServices : IHostedService
{
    [Inject]
    private readonly IMotionCardManager _cardManager;

    [Inject]
    private readonly ILogger<InitializeServices> _logger;

    [TryCatch]
    public async Task StartAsync(CancellationToken cancellationToken)
    {
#if DEBUG
        await LeadshineDebugModeHelper.SetDebugModeAsync(
            DebugOutputMode.All,
            "LeadshineCardDebug.log",
            _logger
        );
#endif

        await _cardManager.InitializeAllCardsAsync();
        _logger.ZLogInformation($"All motion cards initialized successfully.");
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _cardManager.CloseAllAsync();
    }
}
