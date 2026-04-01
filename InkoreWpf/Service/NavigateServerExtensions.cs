using InkoreWpf.View;
using Microsoft.Extensions.DependencyInjection;

namespace InkoreWpf.Service;

/// <summary>
/// NavigateServer 链式扩展方法
/// </summary>
public static class NavigateServerExtensions
{
    /// <summary>
    /// 注册页面映射
    /// </summary>
    public static IServiceCollection AddPage<TPage>(this IServiceCollection services, string tag) where TPage : class
    {
        services.Configure<NavigateServerOptions>(opt => opt.Pages[tag] = typeof(TPage));
        return services;
    }

    /// <summary>
    /// 注册默认页面
    /// </summary>
    public static IServiceCollection AddDefaultPages(this IServiceCollection services)
    {
        return services
            .AddPage<HomeView>("Home")
            .AddPage<SettingsView>("Settings");
    }
}

/// <summary>
/// 导航配置选项
/// </summary>
public class NavigateServerOptions
{
    public Dictionary<string, Type> Pages { get; } = [];
}
