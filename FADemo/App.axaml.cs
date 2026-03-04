using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using FADemo.Views;
using LyuExtensions.Extensions;
using LyuLogExtension.Builder;
using LyuLogExtension.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using ZLogger.Providers;

namespace FADemo;

public partial class App : Application
{
    public IServiceProvider Services { get; private set; } = null!;

    public static T GetService<T>() where T : notnull
    {
        if (Current is not App app)
            throw new InvalidOperationException("Application is not initialized.");

        return app.Services.GetRequiredService<T>();
    }

    public static object GetService(Type serviceType)
    {
        if (Current is not App app)
            throw new InvalidOperationException("Application is not initialized.");

        return app.Services.GetRequiredService(serviceType);
    }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        Services = ConfigureServices();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit.
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();
            desktop.MainWindow = Services.GetRequiredService<MainWindow>();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        //LyuLogExtension
        services.AddZLogger(builder =>
            builder
                .WithRetentionDays(30)
                .WithCleanupInterval(TimeSpan.FromHours(2))
                .FilterMicrosoft()
                .FilterSystem()
                .WithRollingInterval(RollingInterval.Hour)
                .WithRollingSizeKB(1024 * 50)
                .AddInfoOutput() // 默认info以上，logs/
                .AddFileOutput("logs/trace/", LogLevel.Trace)
                .AddFileOutput("logs/debug/", LogLevel.Debug, LogLevel.Debug)
        );

        services.RegisterServices();

        return services.BuildServiceProvider();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}
