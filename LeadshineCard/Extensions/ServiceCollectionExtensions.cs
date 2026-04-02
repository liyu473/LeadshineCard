using LeadshineCard.Core.Interfaces;
using LeadshineCard.Implementation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LeadshineCard.Extensions;

/// <summary>
/// 服务集合扩展方法。
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 注册雷赛单卡运动控制服务。
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="useLogger">是否使用宿主日志</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddLeadshineMotionControl(
        this IServiceCollection services,
        bool useLogger = true
    )
    {
        services.AddSingleton<IMotionCard>(provider =>
        {
            if (useLogger)
            {
                var logger = provider.GetService<ILogger<LeadshineMotionCard>>();
                var loggerFactory = provider.GetService<ILoggerFactory>();
                return new LeadshineMotionCard(logger, loggerFactory);
            }

            return new LeadshineMotionCard(null, null);
        });

        return services;
    }

    /// <summary>
    /// 注册雷赛单卡运动控制服务，并由库侧补充日志配置。
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configureLogging">日志配置</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddLeadshineMotionControlWithLogging(
        this IServiceCollection services,
        Action<ILoggingBuilder>? configureLogging = null
    )
    {
        services.AddLogging(builder =>
        {
            if (configureLogging != null)
            {
                configureLogging(builder);
            }
            else
            {
                builder.AddConsole();
                builder.AddDebug();
                builder.SetMinimumLevel(LogLevel.Information);
            }
        });

        services.AddSingleton<IMotionCard, LeadshineMotionCard>();

        return services;
    }

    /// <summary>
    /// 注册雷赛多板卡管理服务。
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="useLogger">是否使用宿主日志</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddLeadshineMultiMotionControl(
        this IServiceCollection services,
        bool useLogger = true
    )
    {
        services.AddSingleton<IMotionCardManager>(provider =>
        {
            if (useLogger)
            {
                var logger = provider.GetService<ILogger<LeadshineMotionCardManager>>();
                var loggerFactory = provider.GetService<ILoggerFactory>();
                return new LeadshineMotionCardManager(logger, loggerFactory);
            }

            return new LeadshineMotionCardManager(null, null);
        });

        return services;
    }

    /// <summary>
    /// 注册雷赛多板卡管理服务，并由库侧补充日志配置。
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configureLogging">日志配置</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddLeadshineMultiMotionControlWithLogging(
        this IServiceCollection services,
        Action<ILoggingBuilder>? configureLogging = null
    )
    {
        services.AddLogging(builder =>
        {
            if (configureLogging != null)
            {
                configureLogging(builder);
            }
            else
            {
                builder.AddConsole();
                builder.AddDebug();
                builder.SetMinimumLevel(LogLevel.Information);
            }
        });

        services.AddSingleton<IMotionCardManager, LeadshineMotionCardManager>();

        return services;
    }
}
