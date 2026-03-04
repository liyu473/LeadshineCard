using System.Threading.Tasks;
using FluentAvalonia.UI.Controls;

namespace FADemo.Helpers;

public static class MessageBoxHelper
{
    public static void Show(
        string content,
        string title = "提示",
        string primarytext = "确定",
        bool fullsize = false
    )
    {
        var mes = new ContentDialog()
        {
            Content = content,
            Title = title,
            PrimaryButtonText = primarytext,
            FullSizeDesired = fullsize,
        };

        _ = mes.ShowAsync();
    }

    public static async Task<bool> ShowAsync(
        string content,
        string title = "提示",
        string primarytext = "确定",
        string secondary = "取消",
        bool fullsize = false
    )
    {
        var mes = new ContentDialog
        {
            PrimaryButtonText = secondary,
            SecondaryButtonText = primarytext,           
            Title = title,
            Content = content,
            DefaultButton = ContentDialogButton.Secondary,
            FullSizeDesired = fullsize,
        };

        return await mes.ShowAsync() == ContentDialogResult.Secondary;
    }
}
