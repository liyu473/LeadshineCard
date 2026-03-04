using System;
using System.Threading.Tasks;
using FADemo.ViewModels;

namespace FADemo.Helpers;

public static class LoadingHelper
{
    public static async Task ExecuteWithLoadingAsync(Func<Task> asyncAction)
    {
        var viewModel = App.GetService<MainWindowViewModel>();
        try
        {
            viewModel.IsLoading = true;
            await asyncAction();
        }
        finally
        {
            viewModel.IsLoading = false;
        }
    }

    public static async Task<T> ExecuteWithLoadingAsync<T>(Func<Task<T>> asyncFunc)
    {
        var viewModel = App.GetService<MainWindowViewModel>();
        try
        {
            viewModel.IsLoading = true;
            return await asyncFunc();
        }
        finally
        {
            viewModel.IsLoading = false;
        }
    }
}
