using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace InkoreWpf.Service;

public sealed class NavigateServer(IServiceProvider provider, IOptions<NavigateServerOptions> options)
{
    private readonly Dictionary<string, Type> _pageDic = new(options.Value.Pages);

    public bool Remove(string tag) => _pageDic.Remove(tag);

    public object? GetPageByTag(string tag)
    {
        if (_pageDic.TryGetValue(tag, out var type))
        {
            return provider.GetRequiredService(type);
        }
        return null;
    }

    public string? GetHeaderByType(Type pageType)
        => _pageDic.FirstOrDefault(kv => kv.Value == pageType).Key;
}