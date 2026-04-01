using Microsoft.Extensions.DependencyInjection;

namespace InkoreWpf.Service;

internal static class RegisterNavigation
{
    /// <summary>
    /// 注册导航服务
    /// </summary>
    public static IServiceCollection AddNavigation(this IServiceCollection services)
    {
        services.AddSingleton<NavigateServer>();
        services.AddDefaultPages();
        return services;
    }
}
