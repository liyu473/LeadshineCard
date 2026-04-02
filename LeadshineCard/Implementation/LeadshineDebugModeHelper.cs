using System.Runtime.InteropServices;
using LeadshineCard.Core.Enums;
using LeadshineCard.Core.Exceptions;
using LeadshineCard.Core.Models;
using LeadshineCard.ThirdPart;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace LeadshineCard.Implementation;

/// <summary>
/// 雷赛板卡调试模式辅助类。
/// 用于设置和获取函数库的全局调试输出模式。
/// </summary>
public static class LeadshineDebugModeHelper
{
    private const int FileNameBufferSize = 1024;

    /// <summary>
    /// 设置函数库调试输出模式。
    /// 这是全局库设置，不是单张板卡私有设置。
    /// </summary>
    /// <param name="mode">调试输出模式</param>
    /// <param name="fileName">日志文件路径</param>
    /// <param name="logger">日志记录器（可选）</param>
    /// <returns>是否成功</returns>
    public static async Task<bool> SetDebugModeAsync(
        DebugOutputMode mode,
        string fileName,
        ILogger? logger = null
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);

        logger ??= NullLogger.Instance;

        var result = await Task.Run(() => LTDMC.dmc_set_debug_mode((ushort)mode, fileName))
            .ConfigureAwait(false);
        if (result != 0)
        {
            logger.LogError("设置调试输出模式失败，错误码: {ErrorCode}", result);
            return false;
        }

        logger.LogInformation("调试输出模式已设置: {Mode}, 文件: {FileName}", mode, fileName);
        return true;
    }

    /// <summary>
    /// 获取函数库调试输出设置。
    /// 这是全局库设置，不是单张板卡私有设置。
    /// </summary>
    /// <param name="logger">日志记录器（可选）</param>
    /// <returns>调试输出设置</returns>
    public static async Task<DebugModeSettings> GetDebugModeAsync(ILogger? logger = null)
    {
        logger ??= NullLogger.Instance;
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
