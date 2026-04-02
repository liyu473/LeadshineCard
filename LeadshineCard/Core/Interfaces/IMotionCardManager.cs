namespace LeadshineCard.Core.Interfaces;

/// <summary>
/// 多板卡管理器接口。
/// </summary>
public interface IMotionCardManager : IDisposable
{
    /// <summary>
    /// 获取并初始化指定板卡。
    /// </summary>
    /// <param name="cardNo">板卡号</param>
    /// <param name="heartbeat">是否启用心跳</param>
    /// <returns>板卡实例</returns>
    Task<IMotionCard> GetCardAsync(ushort cardNo, bool heartbeat = true);

    /// <summary>
    /// 批量初始化多张板卡。
    /// </summary>
    /// <param name="cardNos">板卡号集合</param>
    /// <param name="heartbeat">是否启用心跳</param>
    Task InitializeCardsAsync(IEnumerable<ushort> cardNos, bool heartbeat = true);

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
