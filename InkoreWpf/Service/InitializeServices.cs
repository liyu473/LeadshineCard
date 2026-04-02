using LeadshineCard.Core.Interfaces;
using LyuExtensions.Aspects;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace InkoreWpf.Service;

[HostedService]
public class InitializeServices : IHostedService
{
    [Inject]
    private readonly IMotionCard _card;

    [Inject]
    private readonly ILogger<InitializeServices> _logger;

    [TryCatch]
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _card.InitializeAsync();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("正在关闭板卡连接...");
        await _card.CloseAsync();
    }
}
