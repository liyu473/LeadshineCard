using LeadshineCard.Core.Interfaces;
using LyuExtensions.Aspects;
using Microsoft.Extensions.Hosting;

namespace InkoreWpf.Service;

[HostedService]
public class InitializeServices : IHostedService
{
    [Inject]
    private readonly IMotionCard _card;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _card.InitializeAsync(0);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _card.CloseAsync();
    }
}
