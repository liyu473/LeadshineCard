using LeadshineCard.Core.Enums;
using LeadshineCard.Core.Models;

namespace LeadshineCard.Core.Interfaces;

/// <summary>
/// 多板卡管理器接口。
/// </summary>
public interface IMotionCardManager : IDisposable
{
    /// <summary>
    /// 获取当前已实例化的控制卡数量。
    /// </summary>
    /// <returns>控制卡数量</returns>
    Task<ushort> GetDetectedCardCountAsync();

    /// <summary>
    /// 获取当前已实例化的控制卡卡号列表。
    /// </summary>
    /// <returns>控制卡卡号列表</returns>
    Task<IReadOnlyList<ushort>> GetDetectedCardNosAsync();

    /// <summary>
    /// 设置函数库调试输出模式。
    /// 这是全局库设置。
    /// </summary>
    /// <param name="mode">调试输出模式</param>
    /// <param name="fileName">日志文件路径</param>
    /// <returns>是否成功</returns>
    Task<bool> SetDebugModeAsync(DebugOutputMode mode, string fileName);

    /// <summary>
    /// 获取函数库调试输出设置。
    /// 这是全局库设置。
    /// </summary>
    /// <returns>调试输出设置</returns>
    Task<DebugModeSettings> GetDebugModeAsync();

    /// <summary>
    /// 获取已实例化的指定板卡。
    /// </summary>
    /// <param name="cardNo">板卡号</param>
    /// <returns>板卡实例</returns>
    Task<IMotionCard> GetCardAsync(ushort cardNo);

    /// <summary>
    /// 初始化全部检测到的板卡，并缓存对应实例。
    /// </summary>
    Task InitializeAllCardsAsync();

    /// <summary>
    /// 获取当前已经初始化的板卡。
    /// </summary>
    /// <returns>已初始化板卡集合</returns>
    IReadOnlyCollection<IMotionCard> GetInitializedCards();

    /// <summary>
    /// 关闭指定板卡。
    /// </summary>
    /// <param name="cardNo">板卡号</param>
    /// <returns>是否关闭成功</returns>
    Task<bool> CloseCardAsync(ushort cardNo);

    /// <summary>
    /// 关闭全部板卡。
    /// </summary>
    Task CloseAllAsync();
}
