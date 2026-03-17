namespace LeadshineCard.Core.Interfaces;

/// <summary>
/// IO控制器接口
/// </summary>
public interface IIoController
{
    /// <summary>
    /// 读取输入位（同步方法，适合高频轮询）
    /// </summary>
    /// <param name="bitNo">位号</param>
    /// <returns>输入状态</returns>
    bool ReadInputBit(ushort bitNo);

    /// <summary>
    /// 写入输出位
    /// </summary>
    /// <param name="bitNo">位号</param>
    /// <param name="value">输出值</param>
    /// <returns>是否成功</returns>
    Task<bool> WriteOutputBitAsync(ushort bitNo, bool value);

    /// <summary>
    /// 读取输出位状态（同步方法，适合高频轮询）
    /// </summary>
    /// <param name="bitNo">位号</param>
    /// <returns>输出状态</returns>
    bool ReadOutputBit(ushort bitNo);

    /// <summary>
    /// 读取输入端口（同步方法，适合高频轮询）
    /// </summary>
    /// <param name="portNo">端口号</param>
    /// <returns>端口值</returns>
    uint ReadInputPort(ushort portNo);

    /// <summary>
    /// 读取输出端口（同步方法，适合高频轮询）
    /// </summary>
    /// <param name="portNo">端口号</param>
    /// <returns>端口值</returns>
    uint ReadOutputPort(ushort portNo);

    /// <summary>
    /// 写入输出端口
    /// </summary>
    /// <param name="portNo">端口号</param>
    /// <param name="value">端口值</param>
    /// <returns>是否成功</returns>
    Task<bool> WriteOutputPortAsync(ushort portNo, uint value);

    /// <summary>
    /// 批量读取输入位
    /// </summary>
    /// <param name="startBit">起始位号</param>
    /// <param name="count">数量</param>
    /// <returns>输入状态数组</returns>
    Task<bool[]> ReadInputBitsAsync(ushort startBit, ushort count);

    /// <summary>
    /// 批量写入输出位
    /// </summary>
    /// <param name="startBit">起始位号</param>
    /// <param name="values">输出值数组</param>
    /// <returns>是否成功</returns>
    Task<bool> WriteOutputBitsAsync(ushort startBit, bool[] values);
}
