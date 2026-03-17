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

    /// <summary>
    /// 读取输入位（同步方法，适合高频轮询）
    /// </summary>
    public bool ReadInputBit(ushort bitNo)
    {
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
}
