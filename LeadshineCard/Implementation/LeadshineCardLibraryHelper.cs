using LeadshineCard.Core.Exceptions;
using LeadshineCard.ThirdPart;
using Microsoft.Extensions.Logging;

namespace LeadshineCard.Implementation;

internal static class LeadshineCardLibraryHelper
{
    private const ushort MaxSupportedCards = 8;

    internal static ushort[] InitializeAndGetDetectedCardNos(ILogger logger)
    {
        var initResult = LTDMC.dmc_board_init();
        if (initResult == 0)
        {
            throw new CardInitializationException("未检测到控制卡", 0);
        }

        if (initResult < 0)
        {
            var duplicatedCardNo = Math.Abs(initResult) - 1;
            throw new CardInitializationException(
                $"检测到重复硬件卡号 {duplicatedCardNo}",
                initResult
            );
        }

        ushort cardCount = 0;
        uint[] cardTypeList = new uint[MaxSupportedCards];
        ushort[] cardIdList = new ushort[MaxSupportedCards];

        var infoResult = LTDMC.dmc_get_CardInfList(ref cardCount, cardTypeList, cardIdList);
        if (infoResult != 0)
        {
            TryClose(logger);
            throw new MotionCardException("获取板卡信息列表失败", infoResult);
        }

        return [.. cardIdList.Take(cardCount)];
    }

    internal static void Close()
    {
        var result = LTDMC.dmc_board_close();
        if (result != 0)
        {
            throw new MotionCardException("关闭全局板卡资源失败", result);
        }
    }

    internal static void TryClose(ILogger logger)
    {
        try
        {
            Close();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "释放全局板卡资源失败");
        }
    }

    internal static void Reset()
    {
        var result = LTDMC.dmc_board_reset();
        if (result != 0)
        {
            throw new MotionCardException("板卡复位失败", result);
        }
    }
}
