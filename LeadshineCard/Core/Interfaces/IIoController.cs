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

    /// <summary>
    /// 设置本地 DA 输出使能（第八章 8.26）
    /// </summary>
    /// <param name="enable">true=使能, false=禁止</param>
    /// <returns>是否成功</returns>
    Task<bool> SetDaEnableAsync(bool enable);

    /// <summary>
    /// 读取本地 DA 输出使能（第八章 8.26）
    /// </summary>
    /// <returns>使能状态，读取失败返回null</returns>
    bool? GetDaEnable();

    /// <summary>
    /// 设置本地 DA 输出（第八章 8.26）
    /// </summary>
    /// <param name="channel">DA通道，范围0~1</param>
    /// <param name="voltage">输出电压，范围-10V~10V</param>
    /// <returns>是否成功</returns>
    Task<bool> SetDaOutputAsync(ushort channel, double voltage);

    /// <summary>
    /// 读取本地 DA 输出（第八章 8.26）
    /// </summary>
    /// <param name="channel">DA通道，范围0~1</param>
    /// <returns>输出电压，读取失败返回null</returns>
    double? GetDaOutput(ushort channel);

    /// <summary>
    /// 读取本地 AD 输入（第八章 8.26）
    /// </summary>
    /// <param name="channel">AD通道，范围0~7</param>
    /// <returns>输入电压，读取失败返回null</returns>
    double? GetAdInput(ushort channel);

    /// <summary>
    /// 获取IO总数
    /// </summary>
    /// <returns>(输入位总数, 输出位总数)，失败返回null</returns>
    (ushort TotalIn, ushort TotalOut)? GetTotalIoNum();

    /// <summary>
    /// 读取所有输入位
    /// </summary>
    /// <returns>所有输入位状态数组，失败返回空数组</returns>
    Task<bool[]> ReadAllInputBitsAsync();

    /// <summary>
    /// 读取所有输出位
    /// </summary>
    /// <returns>所有输出位状态数组，失败返回空数组</returns>
    Task<bool[]> ReadAllOutputBitsAsync();
}
