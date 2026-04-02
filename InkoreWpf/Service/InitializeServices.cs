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
    private readonly IServiceProvider _serviceProvider;

    [Inject]
    private readonly IConfiguration _configuration;

    [Inject]
    private readonly ILogger<InitializeServices> _logger;

    [TryCatch]
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var cardNos = GetCardNos();
        var cardManager = _serviceProvider.GetService<IMotionCardManager>();

        if (cardManager != null)
        {
            _logger.LogInformation("正在初始化多板卡: {CardNos}", string.Join(", ", cardNos));
            await cardManager.InitializeCardsAsync(cardNos);
            return;
        }

        var motionCard = _serviceProvider.GetRequiredService<IMotionCard>();
        var cardNo = cardNos[0];

        if (cardNos.Length > 1)
        {
            _logger.LogWarning(
                "当前仅注册了单卡服务，将只初始化第一张板卡 {CardNo}，其余板卡请改用 AddLeadshineMultiMotionControl",
                cardNo
            );
        }

        _logger.LogInformation("正在初始化单板卡: {CardNo}", cardNo);
        await motionCard.InitializeAsync(cardNo);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("正在关闭板卡连接...");

        var cardManager = _serviceProvider.GetService<IMotionCardManager>();
        if (cardManager != null)
        {
            await cardManager.CloseAllAsync();
            return;
        }

        var motionCard = _serviceProvider.GetService<IMotionCard>();
        if (motionCard != null)
        {
            await motionCard.CloseAsync();
        }
    }

    private ushort[] GetCardNos()
    {
        var configuredCardNos = _configuration
            .GetSection("Leadshine:CardNos")
            .GetChildren()
            .Select(section => section.Value)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => ushort.TryParse(value, out var cardNo) ? cardNo : (ushort?)null)
            .Where(cardNo => cardNo.HasValue)
            .Select(cardNo => cardNo!.Value)
            .Distinct()
            .ToArray();

        return configuredCardNos.Length > 0 ? configuredCardNos : [0];
    }
}
