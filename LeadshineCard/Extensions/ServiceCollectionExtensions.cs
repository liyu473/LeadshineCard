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
    /// 如果宿主未注册日志服务，类库将使用NullLogger
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="useLogger">是否使用日志，false则强制使用NullLogger（即使宿主已注册日志）</param>
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
            else
            {
                return new LeadshineMotionCard(null, null);
            }
        });

        return services;
    }

    /// <summary>
    /// 添加雷赛运动控制服务（类库自带日志配置）
    /// 类库会自动配置Console和Debug日志
    /// 在宿主无日志服务时调用
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configureLogging">日志配置委托</param>
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
}
