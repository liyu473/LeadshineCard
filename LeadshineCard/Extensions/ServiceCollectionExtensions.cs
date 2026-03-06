using LeadshineCard.Core.Interfaces;
using LeadshineCard.Implementation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LeadshineCard.Extensions;

/// <summary>
/// 服务集合扩展方法
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 添加雷赛运动控制服务
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddLeadshineMotionControl(
        this IServiceCollection services)
    {
        // 注册日志
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.AddDebug();
            builder.SetMinimumLevel(LogLevel.Information);
        });

        // 注册核心服务 - 单例模式，整个应用程序生命周期内只有一个实例
        services.AddSingleton<IMotionCard, LeadshineMotionCard>();

        return services;
    }

    /// <summary>
    /// 添加雷赛运动控制服务（带日志配置）
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configureLogging">日志配置委托</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddLeadshineMotionControl(
        this IServiceCollection services,
        Action<ILoggingBuilder> configureLogging)
    {
        ArgumentNullException.ThrowIfNull(configureLogging);

        // 注册日志
        services.AddLogging(configureLogging);

        // 注册核心服务
        services.AddSingleton<IMotionCard, LeadshineMotionCard>();

        return services;
    }

    /// <summary>
    /// 添加雷赛运动控制服务（完整配置）
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configureLogging">日志配置委托</param>
    /// <param name="configureServices">服务配置委托</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddLeadshineMotionControl(
        this IServiceCollection services,
        Action<ILoggingBuilder>? configureLogging = null,
        Action<IServiceCollection>? configureServices = null)
    {
        // 注册日志
        if (configureLogging != null)
        {
            services.AddLogging(configureLogging);
        }
        else
        {
            services.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.AddDebug();
                builder.SetMinimumLevel(LogLevel.Information);
            });
        }

        // 注册核心服务
        services.AddSingleton<IMotionCard, LeadshineMotionCard>();

        // 执行自定义配置
        configureServices?.Invoke(services);

        return services;
    }
}
