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
    public static IServiceCollection AddLeadshineMotionControl(
        this IServiceCollection services,
        bool useLogger = true
    ) => services.AddLeadshineMotionControl(0, useLogger);

    public static IServiceCollection AddLeadshineMotionControl(
        this IServiceCollection services,
        ushort cardNo,
        bool useLogger = true
    )
    {
        services.AddSingleton<IMotionCard>(provider =>
        {
            if (useLogger)
            {
                return new LeadshineMotionCard(
                    cardNo,
                    provider.GetService<ILogger<LeadshineMotionCard>>(),
                    provider.GetService<ILoggerFactory>()
                );
            }

            return new LeadshineMotionCard(cardNo, null, null);
        });

        return services;
    }

    public static IServiceCollection AddLeadshineMotionControlWithLogging(
        this IServiceCollection services,
        Action<ILoggingBuilder>? configureLogging = null
    ) => services.AddLeadshineMotionControlWithLogging(0, configureLogging);

    public static IServiceCollection AddLeadshineMotionControlWithLogging(
        this IServiceCollection services,
        ushort cardNo,
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

        services.AddSingleton<IMotionCard>(provider =>
            new LeadshineMotionCard(
                cardNo,
                provider.GetService<ILogger<LeadshineMotionCard>>(),
                provider.GetService<ILoggerFactory>()
            )
        );

        return services;
    }

    public static IServiceCollection AddLeadshineMultiMotionControl(
        this IServiceCollection services,
        bool useLogger = true
    )
    {
        services.AddSingleton<IMotionCardManager>(provider =>
        {
            if (useLogger)
            {
                return new LeadshineMotionCardManager(
                    provider.GetService<ILogger<LeadshineMotionCardManager>>(),
                    provider.GetService<ILoggerFactory>()
                );
            }

            return new LeadshineMotionCardManager(null, null);
        });

        return services;
    }

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

        services.AddSingleton<IMotionCardManager>(provider =>
            new LeadshineMotionCardManager(
                provider.GetService<ILogger<LeadshineMotionCardManager>>(),
                provider.GetService<ILoggerFactory>()
            )
        );

        return services;
    }
}
