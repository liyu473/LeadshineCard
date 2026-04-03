using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InkoreWpf.Model;
using LyuEModbus.Abstractions;
using LyuEModbus.Extensions;
using LyuEModbus.Models;
using LyuExtensions.Aspects;
using LyuWpfHelper.Extensions;
using LyuWpfHelper.Services;
using LyuWpfHelper.ViewModels;

namespace InkoreWpf.ViewModel;

[Singleton]
public partial class ModbusTcpViewModel(IEModbusFactory factory) : ViewModelBase
{
    [Inject]
    private readonly INotificationService _notification;

    public ObservableCollection<ModbusMasterOptions> MasterOptions { get; set; } = [];

    /// <summary>
    /// Master名称
    /// </summary>
    [ObservableProperty]
    public partial ModbusMasterOptions SelectedMaster { get; set; }

    /// <summary>
    /// 位索引列表 (0-15)
    /// </summary>
    public ObservableCollection<int> BitIndexes { get; } = [.. (Enumerable.Range(0, 16))];

    /// <summary>
    /// 数据类型列表
    /// </summary>
    public ObservableCollection<ModbusDataType> DataTypes { get; } =
        [.. (Enum.GetValues<ModbusDataType>())];

    #region 读取区域

    /// <summary>
    /// 读取-起始地址
    /// </summary>
    [ObservableProperty]
    public partial ushort ReadAddress { get; set; } = 0;

    /// <summary>
    /// 读取-数量
    /// </summary>
    [ObservableProperty]
    public partial int ReadCount { get; set; } = 1;

    /// <summary>
    /// 读取-数据类型
    /// </summary>
    [ObservableProperty]
    public partial ModbusDataType ReadDataType { get; set; } = ModbusDataType.Int16;

    /// <summary>
    /// 读取-结果
    /// </summary>
    [ObservableProperty]
    public partial string ReadResult { get; set; } = string.Empty;

    /// <summary>
    /// 执行读取操作
    /// </summary>
    [RelayCommand]
    private async Task ReadAsync()
    {
        if (SelectedMaster is null)
        {
            _notification.ShowError("无主站配置！");
            return;
        }

        var master = factory.GetMaster(SelectedMaster.Name);
        if (master == null)
        {
            ReadResult = $"错误：未找到Modbus主站 '{SelectedMaster.Name}'";
            return;
        }

        try
        {
            ReadResult = await ReadDataAsync(master);
        }
        catch (Exception ex)
        {
            ReadResult = $"读取失败: {ex.Message}";
        }
    }

    private async Task<string> ReadDataAsync(IModbusMasterClient master)
    {
        return ReadDataType switch
        {
            ModbusDataType.Boolean => await ReadBooleansAsync(master),
            ModbusDataType.Int16 => await ReadInt16sAsync(master),
            ModbusDataType.UInt16 => await ReadUInt16sAsync(master),
            ModbusDataType.Int32 => await ReadInt32sAsync(master),
            ModbusDataType.UInt32 => await ReadUInt32sAsync(master),
            ModbusDataType.Float => await ReadFloatsAsync(master),
            ModbusDataType.Double => await ReadDoublesAsync(master),
            ModbusDataType.Int64 => await ReadInt64sAsync(master),
            ModbusDataType.UInt64 => await ReadUInt64sAsync(master),
            _ => "不支持的数据类型"
        };
    }

    private async Task<string> ReadBooleansAsync(IModbusMasterClient master)
    {
        if (ReadCount == 1)
        {
            var value = await master.ReadBooleanAsync(ReadAddress);
            return value?.ToString() ?? "读取失败";
        }
        var values = await master.ReadBooleansAsync(ReadAddress, ReadCount);
        return values != null ? string.Join(", ", values) : "读取失败";
    }

    private async Task<string> ReadInt16sAsync(IModbusMasterClient master)
    {
        if (ReadCount == 1)
        {
            var value = await master.ReadInt16Async(ReadAddress);
            return value?.ToString() ?? "读取失败";
        }
        var values = await master.ReadInt16sAsync(ReadAddress, (ushort)ReadCount);
        return values != null ? string.Join(", ", values) : "读取失败";
    }

    private async Task<string> ReadUInt16sAsync(IModbusMasterClient master)
    {
        if (ReadCount == 1)
        {
            var value = await master.ReadUInt16Async(ReadAddress);
            return value?.ToString() ?? "读取失败";
        }
        var values = await master.ReadUInt16sAsync(ReadAddress, (ushort)ReadCount);
        return values != null ? string.Join(", ", values) : "读取失败";
    }

    private async Task<string> ReadInt32sAsync(IModbusMasterClient master)
    {
        if (ReadCount == 1)
        {
            var value = await master.ReadInt32Async(ReadAddress);
            return value?.ToString() ?? "读取失败";
        }
        var values = await master.ReadInt32sAsync(ReadAddress, ReadCount);
        return values != null ? string.Join(", ", values) : "读取失败";
    }

    private async Task<string> ReadUInt32sAsync(IModbusMasterClient master)
    {
        if (ReadCount == 1)
        {
            var value = await master.ReadUInt32Async(ReadAddress);
            return value?.ToString() ?? "读取失败";
        }
        var values = await master.ReadUInt32sAsync(ReadAddress, ReadCount);
        return values != null ? string.Join(", ", values) : "读取失败";
    }

    private async Task<string> ReadFloatsAsync(IModbusMasterClient master)
    {
        if (ReadCount == 1)
        {
            var value = await master.ReadFloatAsync(ReadAddress);
            return value?.ToString() ?? "读取失败";
        }
        var values = await master.ReadFloatsAsync(ReadAddress, ReadCount);
        return values != null ? string.Join(", ", values) : "读取失败";
    }

    private async Task<string> ReadDoublesAsync(IModbusMasterClient master)
    {
        if (ReadCount == 1)
        {
            var value = await master.ReadDoubleAsync(ReadAddress);
            return value?.ToString() ?? "读取失败";
        }
        var values = await master.ReadDoublesAsync(ReadAddress, ReadCount);
        return values != null ? string.Join(", ", values) : "读取失败";
    }

    private async Task<string> ReadInt64sAsync(IModbusMasterClient master)
    {
        if (ReadCount == 1)
        {
            var value = await master.ReadInt64Async(ReadAddress);
            return value?.ToString() ?? "读取失败";
        }
        var values = await master.ReadInt64sAsync(ReadAddress, ReadCount);
        return values != null ? string.Join(", ", values) : "读取失败";
    }

    private async Task<string> ReadUInt64sAsync(IModbusMasterClient master)
    {
        if (ReadCount == 1)
        {
            var value = await master.ReadUInt64Async(ReadAddress);
            return value?.ToString() ?? "读取失败";
        }
        var values = await master.ReadUInt64sAsync(ReadAddress, ReadCount);
        return values != null ? string.Join(", ", values) : "读取失败";
    }

    #endregion

    #region 写入区域

    /// <summary>
    /// 写入-起始地址
    /// </summary>
    [ObservableProperty]
    public partial ushort WriteAddress { get; set; } = 0;

    /// <summary>
    /// 写入-数据类型
    /// </summary>
    [ObservableProperty]
    public partial ModbusDataType WriteDataType { get; set; } = ModbusDataType.Int16;

    /// <summary>
    /// 写入-值（字符串，多个值用逗号分隔）
    /// </summary>
    [ObservableProperty]
    public partial string WriteValue { get; set; } = "0";

    /// <summary>
    /// 写入-结果
    /// </summary>
    [ObservableProperty]
    public partial string WriteResult { get; set; } = string.Empty;

    /// <summary>
    /// 执行写入操作
    /// </summary>
    [RelayCommand]
    private async Task WriteAsync()
    {
        if (SelectedMaster is null)
        {
            _notification.ShowError("无主站配置！");
            return;
        }
        var master = factory.GetMaster(SelectedMaster.Name);
        if (master == null)
        {
            WriteResult = $"错误：未找到Modbus主站 '{SelectedMaster.Name}'";
            return;
        }

        try
        {
            var success = await WriteDataAsync(master);
            WriteResult = success ? "写入成功" : "写入失败";
        }
        catch (Exception ex)
        {
            WriteResult = $"写入失败: {ex.Message}";
        }
    }

    private async Task<bool> WriteDataAsync(IModbusMasterClient master)
    {
        return WriteDataType switch
        {
            ModbusDataType.Boolean => await WriteBooleansAsync(master),
            ModbusDataType.Int16 => await WriteInt16sAsync(master),
            ModbusDataType.UInt16 => await WriteUInt16sAsync(master),
            ModbusDataType.Int32 => await WriteInt32sAsync(master),
            ModbusDataType.UInt32 => await WriteUInt32sAsync(master),
            ModbusDataType.Float => await WriteFloatsAsync(master),
            ModbusDataType.Double => await WriteDoublesAsync(master),
            ModbusDataType.Int64 => await WriteInt64sAsync(master),
            ModbusDataType.UInt64 => await WriteUInt64sAsync(master),
            _ => false
        };
    }

    private string[] ParseValues() =>
        WriteValue.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    private async Task<bool> WriteBooleansAsync(IModbusMasterClient master)
    {
        var strValues = ParseValues();
        if (strValues.Length == 1)
        {
            var value = bool.Parse(strValues[0]);
            return await master.WriteBooleanAsync(WriteAddress, value);
        }
        var values = strValues.Select(bool.Parse).ToArray();
        return await master.WriteBooleansAsync(WriteAddress, values);
    }

    private async Task<bool> WriteInt16sAsync(IModbusMasterClient master)
    {
        var strValues = ParseValues();
        if (strValues.Length == 1)
        {
            var value = short.Parse(strValues[0]);
            return await master.WriteInt16Async(WriteAddress, value);
        }
        var values = strValues.Select(short.Parse).ToArray();
        return await master.WriteInt16sAsync(WriteAddress, values);
    }

    private async Task<bool> WriteUInt16sAsync(IModbusMasterClient master)
    {
        var strValues = ParseValues();
        if (strValues.Length == 1)
        {
            var value = ushort.Parse(strValues[0]);
            return await master.WriteUInt16Async(WriteAddress, value);
        }
        var values = strValues.Select(ushort.Parse).ToArray();
        return await master.WriteUInt16sAsync(WriteAddress, values);
    }

    private async Task<bool> WriteInt32sAsync(IModbusMasterClient master)
    {
        var strValues = ParseValues();
        if (strValues.Length == 1)
        {
            var value = int.Parse(strValues[0]);
            return await master.WriteInt32Async(WriteAddress, value);
        }
        var values = strValues.Select(int.Parse).ToArray();
        return await master.WriteInt32sAsync(WriteAddress, values);
    }

    private async Task<bool> WriteUInt32sAsync(IModbusMasterClient master)
    {
        var strValues = ParseValues();
        if (strValues.Length == 1)
        {
            var value = uint.Parse(strValues[0]);
            return await master.WriteUInt32Async(WriteAddress, value);
        }
        var values = strValues.Select(uint.Parse).ToArray();
        return await master.WriteUInt32sAsync(WriteAddress, values);
    }

    private async Task<bool> WriteFloatsAsync(IModbusMasterClient master)
    {
        var strValues = ParseValues();
        if (strValues.Length == 1)
        {
            var value = float.Parse(strValues[0]);
            return await master.WriteFloatAsync(WriteAddress, value);
        }
        var values = strValues.Select(float.Parse).ToArray();
        return await master.WriteFloatsAsync(WriteAddress, values);
    }

    private async Task<bool> WriteDoublesAsync(IModbusMasterClient master)
    {
        var strValues = ParseValues();
        if (strValues.Length == 1)
        {
            var value = double.Parse(strValues[0]);
            return await master.WriteDoubleAsync(WriteAddress, value);
        }
        var values = strValues.Select(double.Parse).ToArray();
        return await master.WriteDoublesAsync(WriteAddress, values);
    }

    private async Task<bool> WriteInt64sAsync(IModbusMasterClient master)
    {
        var strValues = ParseValues();
        if (strValues.Length == 1)
        {
            var value = long.Parse(strValues[0]);
            return await master.WriteInt64Async(WriteAddress, value);
        }
        var values = strValues.Select(long.Parse).ToArray();
        return await master.WriteInt64sAsync(WriteAddress, values);
    }

    private async Task<bool> WriteUInt64sAsync(IModbusMasterClient master)
    {
        var strValues = ParseValues();
        if (strValues.Length == 1)
        {
            var value = ulong.Parse(strValues[0]);
            return await master.WriteUInt64Async(WriteAddress, value);
        }
        var values = strValues.Select(ulong.Parse).ToArray();
        return await master.WriteUInt64sAsync(WriteAddress, values);
    }

    #endregion

    #region 位读取区域

    /// <summary>
    /// 位读取-寄存器地址
    /// </summary>
    [ObservableProperty]
    public partial ushort BitReadAddress { get; set; } = 0;

    /// <summary>
    /// 位读取-位索引
    /// </summary>
    [ObservableProperty]
    public partial int BitReadIndex { get; set; } = 0;

    /// <summary>
    /// 位读取-结果（单个位）
    /// </summary>
    [ObservableProperty]
    public partial string BitReadResult { get; set; } = string.Empty;

    /// <summary>
    /// 位读取-结果（所有16位）
    /// </summary>
    [ObservableProperty]
    public partial string BitReadAllResult { get; set; } = string.Empty;

    /// <summary>
    /// 读取单个位
    /// </summary>
    [RelayCommand]
    private async Task ReadBitAsync()
    {
        if (SelectedMaster is null)
        {
            _notification.ShowError("无主站配置！");
            return;
        }
        var master = factory.GetMaster(SelectedMaster.Name);
        if (master == null)
        {
            BitReadResult = $"错误：未找到Modbus主站 '{SelectedMaster.Name}'";
            return;
        }

        try
        {
            var value = await master.ReadRegisterBitAsync(BitReadAddress, BitReadIndex);
            BitReadResult = value?.ToString() ?? "读取失败";
        }
        catch (Exception ex)
        {
            BitReadResult = $"读取失败: {ex.Message}";
        }
    }

    /// <summary>
    /// 读取寄存器所有16位
    /// </summary>
    [RelayCommand]
    private async Task ReadAllBitsAsync()
    {
        if (SelectedMaster is null)
        {
            _notification.ShowError("无主站配置！");
            return;
        }
        var master = factory.GetMaster(SelectedMaster.Name);
        if (master == null)
        {
            BitReadAllResult = $"错误：未找到Modbus主站 '{SelectedMaster.Name}'";
            return;
        }

        try
        {
            var bits = await master.ReadRegisterBitsAsync(BitReadAddress);
            if (bits != null)
            {
                // 格式化显示：bit15...bit0
                var formatted = string.Join(" ", Enumerable.Reverse(bits).Select((b, i) => $"[{15 - i}]:{(b ? "1" : "0")}"));
                BitReadAllResult = formatted;
            }
            else
            {
                BitReadAllResult = "读取失败";
            }
        }
        catch (Exception ex)
        {
            BitReadAllResult = $"读取失败: {ex.Message}";
        }
    }

    #endregion

    #region 位写入区域

    /// <summary>
    /// 位写入-寄存器地址
    /// </summary>
    [ObservableProperty]
    public partial ushort BitWriteAddress { get; set; } = 0;

    /// <summary>
    /// 位写入-位索引
    /// </summary>
    [ObservableProperty]
    public partial int BitWriteIndex { get; set; } = 0;

    /// <summary>
    /// 位写入-值
    /// </summary>
    [ObservableProperty]
    public partial bool BitWriteValue { get; set; } = false;

    /// <summary>
    /// 位写入-结果
    /// </summary>
    [ObservableProperty]
    public partial string BitWriteResult { get; set; } = string.Empty;

    /// <summary>
    /// 写入单个位
    /// </summary>
    [RelayCommand]
    private async Task WriteBitAsync()
    {
        if (SelectedMaster is null)
        {
            _notification.ShowError("无主站配置！");
            return;
        }
        var master = factory.GetMaster(SelectedMaster.Name);
        if (master == null)
        {
            BitWriteResult = $"错误：未找到Modbus主站 '{SelectedMaster.Name}'";
            return;
        }

        try
        {
            var success = await master.WriteRegisterBitAsync(BitWriteAddress, BitWriteIndex, BitWriteValue);
            BitWriteResult = success ? "写入成功" : "写入失败";
        }
        catch (Exception ex)
        {
            BitWriteResult = $"写入失败: {ex.Message}";
        }
    }

    #endregion

    #region 线圈读取区域

    /// <summary>
    /// 线圈读取-起始地址
    /// </summary>
    [ObservableProperty]
    public partial ushort CoilReadAddress { get; set; } = 0;

    /// <summary>
    /// 线圈读取-数量
    /// </summary>
    [ObservableProperty]
    public partial ushort CoilReadCount { get; set; } = 1;

    /// <summary>
    /// 线圈读取-结果
    /// </summary>
    [ObservableProperty]
    public partial string CoilReadResult { get; set; } = string.Empty;

    /// <summary>
    /// 读取线圈
    /// </summary>
    [RelayCommand]
    private async Task ReadCoilAsync()
    {
        if (SelectedMaster is null)
        {
            _notification.ShowError("无主站配置！");
            return;
        }
        var master = factory.GetMaster(SelectedMaster.Name);
        if (master == null)
        {
            CoilReadResult = $"错误：未找到Modbus主站 '{SelectedMaster.Name}'";
            return;
        }

        try
        {
            if (CoilReadCount == 1)
            {
                var value = await master.ReadCoilAsync(CoilReadAddress);
                CoilReadResult = value?.ToString() ?? "读取失败";
            }
            else
            {
                var values = await master.ReadCoilsAsync(CoilReadAddress, CoilReadCount);
                CoilReadResult = values != null
                    ? string.Join(", ", values.Select((v, i) => $"[{CoilReadAddress + i}]:{(v ? "1" : "0")}"))
                    : "读取失败";
            }
        }
        catch (Exception ex)
        {
            CoilReadResult = $"读取失败: {ex.Message}";
        }
    }

    #endregion

    #region 线圈写入区域

    /// <summary>
    /// 线圈写入-起始地址
    /// </summary>
    [ObservableProperty]
    public partial ushort CoilWriteAddress { get; set; } = 0;

    /// <summary>
    /// 线圈写入-值（字符串，多个值用逗号分隔，支持 true/false 或 1/0）
    /// </summary>
    [ObservableProperty]
    public partial string CoilWriteValue { get; set; } = "true";

    /// <summary>
    /// 线圈写入-结果
    /// </summary>
    [ObservableProperty]
    public partial string CoilWriteResult { get; set; } = string.Empty;

    /// <summary>
    /// 写入线圈
    /// </summary>
    [RelayCommand]
    private async Task WriteCoilAsync()
    {
        if (SelectedMaster is null)
        {
            _notification.ShowError("无主站配置！");
            return;
        }
        var master = factory.GetMaster(SelectedMaster.Name);
        if (master == null)
        {
            CoilWriteResult = $"错误：未找到Modbus主站 '{SelectedMaster.Name}'";
            return;
        }

        try
        {
            var strValues = CoilWriteValue.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var values = strValues.Select(ParseBool).ToArray();

            bool success;
            if (values.Length == 1)
            {
                success = await master.WriteCoilAsync(CoilWriteAddress, values[0]);
            }
            else
            {
                success = await master.WriteCoilsAsync(CoilWriteAddress, values);
            }
            CoilWriteResult = success ? "写入成功" : "写入失败";
        }
        catch (Exception ex)
        {
            CoilWriteResult = $"写入失败: {ex.Message}";
        }
    }

    /// <summary>
    /// 切换线圈状态
    /// </summary>
    [RelayCommand]
    private async Task ToggleCoilAsync()
    {
        if (SelectedMaster is null)
        {
            _notification.ShowError("无主站配置！");
            return;
        }
        var master = factory.GetMaster(SelectedMaster.Name);
        if (master == null)
        {
            CoilWriteResult = $"错误：未找到Modbus主站 '{SelectedMaster.Name}'";
            return;
        }

        try
        {
            var newValue = await master.ToggleCoilAsync(CoilWriteAddress);
            CoilWriteResult = newValue.HasValue ? $"切换成功，当前值: {newValue.Value}" : "切换失败";
        }
        catch (Exception ex)
        {
            CoilWriteResult = $"切换失败: {ex.Message}";
        }
    }

    /// <summary>
    /// 解析布尔值（支持 true/false, 1/0, on/off）
    /// </summary>
    private static bool ParseBool(string value)
    {
        return value.ToLower() switch
        {
            "true" or "1" or "on" => true,
            "false" or "0" or "off" => false,
            _ => bool.Parse(value)
        };
    }

    #endregion
}
