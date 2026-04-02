using System.Runtime.InteropServices;
using LeadshineCard.Core.Enums;
using LeadshineCard.Core.Exceptions;
using LeadshineCard.Core.Models;
using LeadshineCard.ThirdPart;
using Microsoft.Extensions.Logging;

namespace LeadshineCard.Implementation;

internal static class LeadshineDebugModeHelper
{
    private const int FileNameBufferSize = 1024;

    internal static async Task<bool> SetDebugModeAsync(
        DebugOutputMode mode,
        string fileName,
        ILogger logger
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);

        var result = await Task.Run(() => LTDMC.dmc_set_debug_mode((ushort)mode, fileName))
            .ConfigureAwait(false);
        if (result != 0)
        {
            logger.LogError("设置调试输出模式失败，错误码: {ErrorCode}", result);
            return false;
        }

        return true;
    }

    internal static async Task<DebugModeSettings> GetDebugModeAsync(ILogger logger)
    {
        IntPtr fileNameBuffer = Marshal.AllocHGlobal(FileNameBufferSize);

        try
        {
            ushort mode = 0;
            var result = await Task.Run(() => LTDMC.dmc_get_debug_mode(ref mode, fileNameBuffer))
                .ConfigureAwait(false);
            if (result != 0)
            {
                logger.LogError("读取调试输出模式失败，错误码: {ErrorCode}", result);
                throw new MotionCardException("读取调试输出模式失败", result);
            }

            var fileName = Marshal.PtrToStringAnsi(fileNameBuffer) ?? string.Empty;

            return new DebugModeSettings
            {
                Mode = Enum.IsDefined(typeof(DebugOutputMode), mode)
                    ? (DebugOutputMode)mode
                    : (DebugOutputMode)mode,
                FileName = fileName,
            };
        }
        finally
        {
            Marshal.FreeHGlobal(fileNameBuffer);
        }
    }
}
