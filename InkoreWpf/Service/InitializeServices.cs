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

        var success = await _cardManager.InitializeAllCardsAsync();
        var list = _cardManager.GetInitializedCards();
        if (success)
            _logger.ZLogInformation($"All motion cards(count：{list.Count}) initialized successfully.");
        else
        {
            _logger.ZLogError($"Failed to initialize all motion cards.");
            _logger.ZLogError($"Succcess : {string.Join(", ", list)}");
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.ZLogInformation($"程序准备退出");
        _logger.ZLogInformation($"Closing all motion cards...");
        var success = await _cardManager.CloseAllAsync();
        if (success)
            _logger.ZLogInformation($"All motion cards closed successfully.");
        else
            _logger.ZLogError($"Failed to close all motion cards.");
    }
}
