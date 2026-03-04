using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using LyuExtensions.Aspects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace FADemo.Services;

[Singleton(ServiceType = typeof(INavigationPageFactory))]
public sealed class NavServiceFactory(IServiceProvider sp) : INavigationPageFactory
{
    private readonly Dictionary<Type, Type> _vmToPage = [];

    public void Register<TViewModel, TPage>()
        where TPage : Control
    {
        _vmToPage[typeof(TViewModel)] = typeof(TPage);
    }

    public Control? GetPage(Type srcType)
    {
        // 如果本身就是 Control 类型
        if (typeof(Control).IsAssignableFrom(srcType))
        {
            return (Control)(sp.GetService(srcType)
                ?? Activator.CreateInstance(srcType)!);
        }

        // 如果不是 Control，按 VM->Page 映射找
        if (_vmToPage.TryGetValue(srcType, out var pageType))
        {
            var page = (Control)(sp.GetService(pageType)
                ?? Activator.CreateInstance(pageType)!);
            return page;
        }

        return null;
    }

    public Control? GetPageFromObject(object target)
    {
        if (target is null)
            return null;

        if (target is Control pageInstance)
            return pageInstance;

        if (_vmToPage.TryGetValue(target.GetType(), out var pageType))
        {
            var page = (Control)(sp.GetService(pageType)
                ?? Activator.CreateInstance(pageType)!);

            page.DataContext = target;
            return page;
        }

        return null;
    }
}

public static class NavServiceFactoryExtensions
{
    public static void RegistersPages(this INavigationPageFactory factory)
    {
        var assembly = Assembly.GetExecutingAssembly();

        var rootNamespace = assembly.GetName().Name;
        var viewModelsNamespace = $"{rootNamespace}.ViewModels";
        var viewsNamespace = $"{rootNamespace}.Views";

        // 获取所有 ViewModel 类型
        var viewModels = assembly.GetTypes()
            .Where(t => t.Namespace == viewModelsNamespace
                && t.IsClass
                && !t.IsAbstract
                && t.Name.EndsWith("ViewModel"))
            .ToList();

        // 获取所有 View 类型（继承自 Control）
        var views = assembly.GetTypes()
            .Where(t => t.Namespace == viewsNamespace
                && t.IsClass
                && !t.IsAbstract
                && typeof(Control).IsAssignableFrom(t))
            .ToList();

        // 自动匹配并注册
        foreach (var viewModel in viewModels)
        {
            var viewModelName = viewModel.Name;
            var expectedViewName = viewModelName.Replace("ViewModel", "");

            var matchedView = views.FirstOrDefault(v => v.Name == expectedViewName);

            if (matchedView != null)
            {
                var registerMethod = typeof(NavServiceFactory)
                    .GetMethod(nameof(NavServiceFactory.Register))!
                    .MakeGenericMethod(viewModel, matchedView);

                registerMethod.Invoke(factory, null);
            }
        }
    }
}