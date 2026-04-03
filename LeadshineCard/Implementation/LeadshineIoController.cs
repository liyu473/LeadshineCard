using LeadshineCard.Core.Interfaces;
using LeadshineCard.ThirdPart;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace LeadshineCard.Implementation;

/// <summary>
/// 雷赛IO控制器实现
/// </summary>
/// <remarks>
/// 构造函数
/// </remarks>
/// <param name="cardNo">板卡号</param>
/// <param name="logger">日志记录器，为null时使用NullLogger</param>
public class LeadshineIoController(ushort cardNo, ILogger<LeadshineIoController>? logger = null)
    : IIoController
{
    private readonly ILogger<LeadshineIoController> _logger =
        logger ?? NullLogger<LeadshineIoController>.Instance;
    private const ushort MinBitNo = 0;
    private const ushort MaxBitNo = 15;
    private const ushort MinInputPortNo = 0;
    private const ushort MaxInputPortNo = 1;
    private const ushort OutputPortNo = 0;
    private const ushort MinDaChannel = 0;
    private const ushort MaxDaChannel = 1;
    private const ushort MinAdChannel = 0;
    private const ushort MaxAdChannel = 7;
    private const double MinDaVoltage = -10.0;
    private const double MaxDaVoltage = 10.0;

    /// <summary>
    /// 读取输入位（同步方法，适合高频轮询）
    /// </summary>
    public bool ReadInputBit(ushort bitNo)
    {
        ValidateBitNo(bitNo, nameof(bitNo));

        try
        {
            var result = LTDMC.dmc_read_inbit(cardNo, bitNo);
            return result == 1;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "读取输入位 {BitNo} 异常", bitNo);
            return false;
        }
    }

    /// <summary>
    /// 写入输出位
    /// </summary>
    public async Task<bool> WriteOutputBitAsync(ushort bitNo, bool value)
    {
        ValidateBitNo(bitNo, nameof(bitNo));
        _logger.LogDebug("写入输出位 {BitNo} = {Value}", bitNo, value);

        try
        {
            ushort val = (ushort)(value ? 1 : 0);
            var result = await Task.Run(() => LTDMC.dmc_write_outbit(cardNo, bitNo, val));

            if (result != 0)
            {
                _logger.LogError("写入输出位 {BitNo} 失败，错误码: {ErrorCode}", bitNo, result);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "写入输出位 {BitNo} 异常", bitNo);
            return false;
        }
    }

    /// <summary>
    /// 读取输出位状态（同步方法，适合高频轮询）
    /// </summary>
    public bool ReadOutputBit(ushort bitNo)
    {
        ValidateBitNo(bitNo, nameof(bitNo));

        try
        {
            var result = LTDMC.dmc_read_outbit(cardNo, bitNo);
            return result == 1;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "读取输出位 {BitNo} 异常", bitNo);
            return false;
        }
    }

    /// <summary>
    /// 读取输入端口（同步方法，适合高频轮询）
    /// </summary>
    public uint ReadInputPort(ushort portNo)
    {
        ValidateInputPortNo(portNo, nameof(portNo));

        try
        {
            var result = LTDMC.dmc_read_inport(cardNo, portNo);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "读取输入端口 {PortNo} 异常", portNo);
            return 0;
        }
    }

    /// <summary>
    /// 读取输出端口（同步方法，适合高频轮询）
    /// </summary>
    public uint ReadOutputPort(ushort portNo)
    {
        ValidateOutputPortNo(portNo, nameof(portNo));

        try
        {
            var result = LTDMC.dmc_read_outport(cardNo, portNo);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "读取输出端口 {PortNo} 异常", portNo);
            return 0;
        }
    }

    /// <summary>
    /// 写入输出端口
    /// </summary>
    public async Task<bool> WriteOutputPortAsync(ushort portNo, uint value)
    {
        ValidateOutputPortNo(portNo, nameof(portNo));
        _logger.LogDebug("写入输出端口 {PortNo} = 0x{Value:X}", portNo, value);

        try
        {
            var result = await Task.Run(() => LTDMC.dmc_write_outport(cardNo, portNo, value));

            if (result != 0)
            {
                _logger.LogError("写入输出端口 {PortNo} 失败，错误码: {ErrorCode}", portNo, result);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "写入输出端口 {PortNo} 异常", portNo);
            return false;
        }
    }

    /// <summary>
    /// 批量读取输入位
    /// </summary>
    public async Task<bool[]> ReadInputBitsAsync(ushort startBit, ushort count)
    {
        if (count == 0)
            throw new ArgumentException("数量必须大于0", nameof(count));
        ValidateBitNo(startBit, nameof(startBit));
        var lastBit = (int)startBit + count - 1;
        if (lastBit > MaxBitNo)
        {
            throw new ArgumentOutOfRangeException(
                nameof(count),
                count,
                $"读取位范围超出限制，最大位号为 {MaxBitNo}"
            );
        }
        _logger.LogDebug("批量读取输入位，起始: {StartBit}, 数量: {Count}", startBit, count);

        try
        {
            var results = new bool[count];
            var tasks = new Task<bool>[count];

            // 并行读取
            for (ushort i = 0; i < count; i++)
            {
                var bitNo = (ushort)(startBit + i);
                tasks[i] = Task.Run(() => ReadInputBit(bitNo)); // 包装同步方法
            }

            var taskResults = await Task.WhenAll(tasks);
            Array.Copy(taskResults, results, count);

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量读取输入位异常");
            return new bool[count];
        }
    }

    /// <summary>
    /// 批量写入输出位
    /// </summary>
    public async Task<bool> WriteOutputBitsAsync(ushort startBit, bool[] values)
    {
        if (values == null || values.Length == 0)
            throw new ArgumentException("值数组不能为空", nameof(values));
        ValidateBitNo(startBit, nameof(startBit));
        var lastBit = (int)startBit + values.Length - 1;
        if (lastBit > MaxBitNo)
        {
            throw new ArgumentOutOfRangeException(
                nameof(values),
                values.Length,
                $"写入位范围超出限制，最大位号为 {MaxBitNo}"
            );
        }

        _logger.LogDebug(
            "批量写入输出位，起始: {StartBit}, 数量: {Count}",
            startBit,
            values.Length
        );

        try
        {
            var tasks = new Task<bool>[values.Length];

            // 并行写入
            for (ushort i = 0; i < values.Length; i++)
            {
                var bitNo = (ushort)(startBit + i);
                tasks[i] = WriteOutputBitAsync(bitNo, values[i]);
            }

            var results = await Task.WhenAll(tasks);

            // 检查是否全部成功
            var allSuccess = results.All(r => r);

            if (!allSuccess && _logger.IsEnabled(LogLevel.Warning))
                _logger.LogWarning("部分输出位写入失败");

            return allSuccess;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量写入输出位异常");
            return false;
        }
    }

    /// <summary>
    /// 并行读取多个输入位
    /// </summary>
    public async Task<bool[]> ReadInputBitsParallelAsync(ushort[] bitNumbers)
    {
        if (bitNumbers == null || bitNumbers.Length == 0)
            throw new ArgumentException("位号数组不能为空", nameof(bitNumbers));
        foreach (var bitNo in bitNumbers)
        {
            ValidateBitNo(bitNo, nameof(bitNumbers));
        }
        _logger.LogDebug("并行读取 {Count} 个输入位", bitNumbers.Length);

        try
        {
            var tasks = bitNumbers.Select(bitNo => Task.Run(() => ReadInputBit(bitNo))).ToArray();
            return await Task.WhenAll(tasks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "并行读取输入位异常");
            return new bool[bitNumbers.Length];
        }
    }

    /// <summary>
    /// 并行写入多个输出位
    /// </summary>
    public async Task<bool> WriteOutputBitsParallelAsync(ushort[] bitNumbers, bool[] values)
    {
        if (bitNumbers == null || bitNumbers.Length == 0)
            throw new ArgumentException("位号数组不能为空", nameof(bitNumbers));

        if (values == null || values.Length != bitNumbers.Length)
            throw new ArgumentException("值数组长度必须与位号数组相同", nameof(values));
        foreach (var bitNo in bitNumbers)
        {
            ValidateBitNo(bitNo, nameof(bitNumbers));
        }
        _logger.LogDebug("并行写入 {Count} 个输出位", bitNumbers.Length);

        try
        {
            var tasks = new Task<bool>[bitNumbers.Length];

            for (int i = 0; i < bitNumbers.Length; i++)
            {
                tasks[i] = WriteOutputBitAsync(bitNumbers[i], values[i]);
            }

            var results = await Task.WhenAll(tasks);
            var allSuccess = results.All(r => r);

            if (!allSuccess && _logger.IsEnabled(LogLevel.Warning))
                _logger.LogWarning("部分输出位写入失败");

            return allSuccess;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "并行写入输出位异常");
            return false;
        }
    }

    /// <summary>
    /// 设置本地 DA 输出使能（第八章 8.26）
    /// </summary>
    public async Task<bool> SetDaEnableAsync(bool enable)
    {
        _logger.LogDebug("设置 DA 使能状态: {Enable}", enable);

        try
        {
            var enableFlag = (ushort)(enable ? 1 : 0);
            var result = await Task.Run(() => LTDMC.dmc_set_da_enable(cardNo, enableFlag));

            if (result != 0)
            {
                _logger.LogError("设置 DA 使能失败，错误码: {ErrorCode}", result);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "设置 DA 使能异常");
            return false;
        }
    }

    /// <summary>
    /// 读取本地 DA 输出使能（第八章 8.26）
    /// </summary>
    public bool? GetDaEnable()
    {
        try
        {
            ushort enable = 0;
            var result = LTDMC.dmc_get_da_enable(cardNo, ref enable);
            if (result != 0)
            {
                _logger.LogError("读取 DA 使能失败，错误码: {ErrorCode}", result);
                return null;
            }

            return enable == 1;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "读取 DA 使能异常");
            return null;
        }
    }

    /// <summary>
    /// 设置本地 DA 输出（第八章 8.26）
    /// </summary>
    public async Task<bool> SetDaOutputAsync(ushort channel, double voltage)
    {
        ValidateDaChannel(channel, nameof(channel));
        ValidateDaVoltage(voltage, nameof(voltage));
        _logger.LogDebug("设置 DA 输出: Channel={Channel}, Voltage={Voltage}", channel, voltage);

        try
        {
            var result = await Task.Run(() => LTDMC.dmc_set_da_output(cardNo, channel, voltage));
            if (result != 0)
            {
                _logger.LogError("设置 DA 输出失败，错误码: {ErrorCode}", result);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "设置 DA 输出异常");
            return false;
        }
    }

    /// <summary>
    /// 读取本地 DA 输出（第八章 8.26）
    /// </summary>
    public double? GetDaOutput(ushort channel)
    {
        ValidateDaChannel(channel, nameof(channel));

        try
        {
            double vout = 0;
            var result = LTDMC.dmc_get_da_output(cardNo, channel, ref vout);
            if (result != 0)
            {
                _logger.LogError("读取 DA 输出失败，错误码: {ErrorCode}", result);
                return null;
            }

            return vout;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "读取 DA 输出异常");
            return null;
        }
    }

    /// <summary>
    /// 读取本地 AD 输入（第八章 8.26）
    /// </summary>
    public double? GetAdInput(ushort channel)
    {
        ValidateAdChannel(channel, nameof(channel));

        try
        {
            double vout = 0;
            var result = LTDMC.dmc_get_ad_input(cardNo, channel, ref vout);
            if (result != 0)
            {
                _logger.LogError("读取 AD 输入失败，错误码: {ErrorCode}", result);
                return null;
            }

            return vout;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "读取 AD 输入异常");
            return null;
        }
    }


    private static void ValidateBitNo(ushort bitNo, string paramName)
    {
        if (bitNo < MinBitNo || bitNo > MaxBitNo)
        {
            throw new ArgumentOutOfRangeException(
                paramName,
                bitNo,
                $"bitNo 超出范围，允许范围为 {MinBitNo}~{MaxBitNo}"
            );
        }
    }

    private static void ValidateInputPortNo(ushort portNo, string paramName)
    {
        if (portNo < MinInputPortNo || portNo > MaxInputPortNo)
        {
            throw new ArgumentOutOfRangeException(
                paramName,
                portNo,
                $"输入端口号超出范围，允许范围为 {MinInputPortNo}~{MaxInputPortNo}"
            );
        }
    }

    private static void ValidateOutputPortNo(ushort portNo, string paramName)
    {
        if (portNo != OutputPortNo)
        {
            throw new ArgumentOutOfRangeException(
                paramName,
                portNo,
                $"输出端口号固定为 {OutputPortNo}"
            );
        }
    }

    private static void ValidateDaChannel(ushort channel, string paramName)
    {
        if (channel < MinDaChannel || channel > MaxDaChannel)
        {
            throw new ArgumentOutOfRangeException(
                paramName,
                channel,
                $"DA 通道超出范围，允许范围为 {MinDaChannel}~{MaxDaChannel}"
            );
        }
    }

    private static void ValidateAdChannel(ushort channel, string paramName)
    {
        if (channel < MinAdChannel || channel > MaxAdChannel)
        {
            throw new ArgumentOutOfRangeException(
                paramName,
                channel,
                $"AD 通道超出范围，允许范围为 {MinAdChannel}~{MaxAdChannel}"
            );
        }
    }

    private static void ValidateDaVoltage(double voltage, string paramName)
    {
        if (double.IsNaN(voltage) || double.IsInfinity(voltage))
        {
            throw new ArgumentException("电压值无效", paramName);
        }

        if (voltage < MinDaVoltage || voltage > MaxDaVoltage)
        {
            throw new ArgumentOutOfRangeException(
                paramName,
                voltage,
                $"DA 输出电压超出范围，允许范围为 {MinDaVoltage}V~{MaxDaVoltage}V"
            );
        }
    }

    /// <summary>
    /// 获取IO总数
    /// </summary>
    public (ushort TotalIn, ushort TotalOut)? GetTotalIoNum()
    {
        try
        {
            ushort totalIn = 0;
            ushort totalOut = 0;
            var result = LTDMC.dmc_get_total_ionum(cardNo, ref totalIn, ref totalOut);

            if (result != 0)
            {
                _logger.LogError("获取IO总数失败，错误码: {ErrorCode}", result);
                return null;
            }

            _logger.LogDebug("IO总数: 输入={TotalIn}, 输出={TotalOut}", totalIn, totalOut);
            return (totalIn, totalOut);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取IO总数异常");
            return null;
        }
    }

    /// <summary>
    /// 读取所有输入位
    /// </summary>
    public async Task<bool[]> ReadAllInputBitsAsync()
    {
        try
        {
            var ioNum = GetTotalIoNum();
            if (!ioNum.HasValue)
            {
                _logger.LogError("无法获取IO总数");
                return [];
            }

            var totalIn = ioNum.Value.TotalIn;
            _logger.LogDebug("开始读取所有输入位，总数: {TotalIn}", totalIn);

            var results = new bool[totalIn];

            // 按端口批量读取（每个端口32位）
            var portCount = (totalIn + 31) / 32;

            await Task.Run(() =>
            {
                for (ushort port = 0; port < portCount; port++)
                {
                    try
                    {
                        var portValue = LTDMC.dmc_read_inport(cardNo, port);

                        // 解析端口的每一位
                        var startBit = port * 32;
                        var endBit = Math.Min(startBit + 32, totalIn);

                        for (int bit = startBit; bit < endBit; bit++)
                        {
                            var bitOffset = bit - startBit;
                            results[bit] = (portValue & (1u << bitOffset)) != 0;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "读取输入端口 {Port} 异常", port);
                    }
                }
            });

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "读取所有输入位异常");
            return [];
        }
    }

    /// <summary>
    /// 读取所有输出位
    /// </summary>
    public async Task<bool[]> ReadAllOutputBitsAsync()
    {
        try
        {
            var ioNum = GetTotalIoNum();
            if (!ioNum.HasValue)
            {
                _logger.LogError("无法获取IO总数");
                return [];
            }

            var totalOut = ioNum.Value.TotalOut;
            _logger.LogDebug("开始读取所有输出位，总数: {TotalOut}", totalOut);

            var results = new bool[totalOut];

            // 按端口批量读取（每个端口32位）
            var portCount = (totalOut + 31) / 32;

            await Task.Run(() =>
            {
                for (ushort port = 0; port < portCount; port++)
                {
                    try
                    {
                        var portValue = LTDMC.dmc_read_outport(cardNo, port);

                        // 解析端口的每一位
                        var startBit = port * 32;
                        var endBit = Math.Min(startBit + 32, totalOut);

                        for (int bit = startBit; bit < endBit; bit++)
                        {
                            var bitOffset = bit - startBit;
                            results[bit] = (portValue & (1u << bitOffset)) != 0;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "读取输出端口 {Port} 异常", port);
                    }
                }
            });

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "读取所有输出位异常");
            return [];
        }
    }

}
