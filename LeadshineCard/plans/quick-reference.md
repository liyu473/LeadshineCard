# 雷赛运动控制卡快速参考指南

## 📋 目录

1. [快速开始](#快速开始)
2. [核心概念](#核心概念)
3. [常用代码片段](#常用代码片段)
4. [配置示例](#配置示例)
5. [故障排查](#故障排查)
6. [最佳实践](#最佳实践)

---

## 🚀 快速开始

### 第一步：添加NuGet包

```bash
dotnet add package Microsoft.Extensions.DependencyInjection
dotnet add package Microsoft.Extensions.Hosting
dotnet add package Microsoft.Extensions.Logging
dotnet add package Microsoft.Extensions.Configuration.Json
```

### 第二步：配置服务

```csharp
var builder = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddLeadshineMotionControl(context.Configuration);
    });

var host = builder.Build();
```

### 第三步：使用服务

```csharp
var motionService = host.Services.GetRequiredService<MotionControlService>();
await motionService.InitializeAsync();
await motionService.MoveAxisAsync(0, 100.0);
```

---

## 💡 核心概念

### 接口层次

```
IMotionCard (板卡)
    ├── IAxisController (轴控制)
    ├── IIoController (IO控制)
    ├── IInterpolationController (插补控制)
    ├── IEncoderController (编码器)
    └── IHomeController (回零)
```

### 服务层次

```
MotionControlService (总控服务)
    ├── AxisService (轴服务)
    ├── IoService (IO服务)
    └── ConfigurationService (配置服务)
```

---

## 📝 常用代码片段

### 1. 初始化板卡

```csharp
// 方式1：直接使用接口
var motionCard = serviceProvider.GetRequiredService<IMotionCard>();
await motionCard.InitializeAsync(0);

// 方式2：使用服务
var motionService = serviceProvider.GetRequiredService<MotionControlService>();
await motionService.InitializeAsync();
```

### 2. 单轴运动

```csharp
// 获取轴控制器
var axis = motionCard.GetAxisController(0);

// 设置运动参数
await axis.SetMotionParametersAsync(new MotionParameters
{
    MaxSpeed = 100.0,
    Acceleration = 500.0,
    Deceleration = 500.0
});

// 相对运动
await axis.MoveRelativeAsync(50.0);

// 绝对运动
await axis.MoveAbsoluteAsync(100.0);

// 等待运动完成
while (!await axis.CheckDoneAsync())
{
    await Task.Delay(10);
}

// 读取位置
var position = await axis.GetCurrentPositionAsync();
```

### 3. JOG运动

```csharp
var axis = motionCard.GetAxisController(0);

// 正向JOG
await axis.JogAsync(true);

// 停止
await axis.StopAsync(StopMode.Deceleration);
```

### 4. IO操作

```csharp
var io = motionCard.GetIoController();

// 读取输入
var input = await io.ReadInputBitAsync(0);

// 写入输出
await io.WriteOutputBitAsync(0, true);

// 批量读取
var inputs = await io.ReadInputBitsAsync(0, 8);

// 读取端口
var portValue = await io.ReadInputPortAsync(0);
```

### 5. 直线插补

```csharp
var interpolation = motionCard.GetInterpolationController();

// 两轴直线插补
await interpolation.LineInterpolationAsync(
    new ushort[] { 0, 1 },
    new double[] { 100.0, 100.0 }
);
```

### 6. 圆弧插补

```csharp
var interpolation = motionCard.GetInterpolationController();

// 圆弧插补（圆心终点式）
await interpolation.ArcInterpolationAsync(
    new ushort[] { 0, 1 },
    new double[] { 100.0, 0.0 },    // 终点
    new double[] { 50.0, 0.0 },     // 圆心
    true                             // 顺时针
);
```

### 7. 连续插补

```csharp
var interpolation = motionCard.GetInterpolationController();
ushort crd = 0;

// 打开缓冲区
await interpolation.OpenContinuousBufferAsync(crd, new ushort[] { 0, 1 });

// 添加轨迹段
await interpolation.AddLineSegmentAsync(crd, new double[] { 100, 100 });
await interpolation.AddArcSegmentAsync(crd, 
    new double[] { 200, 100 }, 
    new double[] { 150, 100 }, 
    true);
await interpolation.AddLineSegmentAsync(crd, new double[] { 200, 0 });

// 开始插补
await interpolation.StartContinuousAsync(crd);

// 监控缓冲区
while (true)
{
    var space = await interpolation.GetRemainingBufferSpaceAsync(crd);
    if (space > 10)
    {
        // 可以继续添加轨迹段
    }
    await Task.Delay(10);
}
```

### 8. 回零

```csharp
var home = motionCard.GetHomeController();

// 配置回零参数
await home.ConfigureAsync(0, new HomeParameters
{
    Direction = HomeDirection.Negative,
    HighSpeed = 50.0,
    LowSpeed = 10.0,
    Mode = HomeMode.HomeSwitch
});

// 执行回零
await home.StartAsync(0);

// 等待回零完成
while (!await home.CheckDoneAsync(0))
{
    await Task.Delay(100);
}
```

### 9. 异常处理

```csharp
try
{
    await axis.MoveRelativeAsync(100.0);
}
catch (AxisLimitException ex)
{
    _logger.LogError("轴触发限位: {Message}", ex.Message);
    // 处理限位异常
}
catch (AxisMotionException ex)
{
    _logger.LogError("轴运动失败: {Message}", ex.Message);
    // 处理运动异常
}
catch (MotionCardException ex)
{
    _logger.LogError("板卡异常: {Message}", ex.Message);
    // 处理板卡异常
}
```

### 10. 状态监控

```csharp
var axis = motionCard.GetAxisController(0);

// 获取轴状态
var status = await axis.GetStatusAsync();

Console.WriteLine($"位置: {status.Position}");
Console.WriteLine($"速度: {status.Speed}");
Console.WriteLine($"状态: {status.State}");
Console.WriteLine($"正限位: {status.PositiveLimit}");
Console.WriteLine($"负限位: {status.NegativeLimit}");
Console.WriteLine($"原点: {status.Home}");
Console.WriteLine($"报警: {status.Alarm}");
```

---

## ⚙️ 配置示例

### appsettings.json

```json
{
  "MotionControl": {
    "Card": {
      "CardNo": 0,
      "MaxAxes": 4,
      "EnableAutoReconnect": true,
      "ReconnectInterval": 5000
    },
    "Axes": [
      {
        "AxisNo": 0,
        "Name": "X轴",
        "MaxSpeed": 100.0,
        "Acceleration": 500.0,
        "Deceleration": 500.0,
        "PulseEquivalent": 0.001,
        "SoftLimitPositive": 1000.0,
        "SoftLimitNegative": -1000.0,
        "EnableSoftLimit": true
      },
      {
        "AxisNo": 1,
        "Name": "Y轴",
        "MaxSpeed": 100.0,
        "Acceleration": 500.0,
        "Deceleration": 500.0,
        "PulseEquivalent": 0.001,
        "SoftLimitPositive": 800.0,
        "SoftLimitNegative": -800.0,
        "EnableSoftLimit": true
      }
    ],
    "Logging": {
      "LogLevel": {
        "Default": "Information",
        "LeadshineCard": "Debug",
        "Microsoft": "Warning"
      },
      "EnableFileLogging": true,
      "LogFilePath": "logs/motion-{Date}.log"
    }
  }
}
```

### 代码中使用配置

```csharp
public class AxisService
{
    private readonly IOptions<AxisConfiguration> _config;
    
    public AxisService(IOptions<AxisConfiguration> config)
    {
        _config = config;
    }
    
    public async Task InitializeAxisAsync(ushort axisNo)
    {
        var axisConfig = _config.Value.Axes
            .FirstOrDefault(a => a.AxisNo == axisNo);
            
        if (axisConfig == null)
        {
            throw new ArgumentException($"未找到轴 {axisNo} 的配置");
        }
        
        // 使用配置初始化轴
        await SetMotionParametersAsync(new MotionParameters
        {
            MaxSpeed = axisConfig.MaxSpeed,
            Acceleration = axisConfig.Acceleration,
            Deceleration = axisConfig.Deceleration
        });
    }
}
```

---

## 🔧 故障排查

### 问题1：板卡初始化失败

**症状**：调用 `InitializeAsync` 返回 false 或抛出异常

**可能原因**：
- 板卡未正确连接
- 驱动未安装
- 板卡号错误
- 其他程序占用板卡

**解决方法**：
```csharp
// 检查板卡连接
var cardInfo = motionCard.GetCardInfo();
if (cardInfo == null)
{
    _logger.LogError("无法获取板卡信息，请检查连接");
}

// 尝试重置板卡
await motionCard.ResetAsync();
await Task.Delay(1000);
await motionCard.InitializeAsync(0);
```

### 问题2：轴运动不响应

**症状**：发送运动命令后轴不动

**可能原因**：
- 伺服未使能
- 限位触发
- 报警状态
- 参数设置错误

**解决方法**：
```csharp
// 检查轴状态
var status = await axis.GetStatusAsync();

if (status.Alarm)
{
    _logger.LogWarning("轴处于报警状态");
    // 清除报警
}

if (status.PositiveLimit || status.NegativeLimit)
{
    _logger.LogWarning("轴触发限位");
    // 反向运动脱离限位
}

// 检查伺服使能
// 根据具体硬件配置使能伺服
```

### 问题3：运动不平滑

**症状**：轴运动有抖动或不连续

**可能原因**：
- 加速度设置过大
- S曲线参数不合适
- 脉冲当量设置错误

**解决方法**：
```csharp
// 调整运动参数
await axis.SetMotionParametersAsync(new MotionParameters
{
    MaxSpeed = 50.0,        // 降低速度
    Acceleration = 200.0,   // 降低加速度
    Deceleration = 200.0,
    SCurveTime = 0.1        // 增加S曲线时间
});
```

### 问题4：位置不准确

**症状**：实际位置与目标位置有偏差

**可能原因**：
- 脉冲当量设置错误
- 反向间隙未补偿
- 编码器未配置

**解决方法**：
```csharp
// 设置正确的脉冲当量
await axis.SetPulseEquivalentAsync(0.001); // 1脉冲 = 0.001mm

// 设置反向间隙补偿
await axis.SetBacklashAsync(0.05); // 补偿0.05mm

// 配置编码器
var encoder = motionCard.GetEncoderController();
await encoder.ConfigureAsync(0, new EncoderConfig
{
    Mode = EncoderMode.AB_4X,
    Equivalent = 0.001
});
```

---

## ✅ 最佳实践

### 1. 资源管理

```csharp
// 使用 using 确保资源释放
using var motionCard = serviceProvider.GetRequiredService<IMotionCard>();
await motionCard.InitializeAsync(0);

// 或使用 try-finally
IMotionCard motionCard = null;
try
{
    motionCard = serviceProvider.GetRequiredService<IMotionCard>();
    await motionCard.InitializeAsync(0);
    // 使用板卡
}
finally
{
    motionCard?.Dispose();
}
```

### 2. 异步编程

```csharp
// ✅ 推荐：使用异步方法
await axis.MoveRelativeAsync(100.0);

// ❌ 不推荐：阻塞等待
axis.MoveRelativeAsync(100.0).Wait();

// ✅ 推荐：并行执行多个操作
var tasks = new[]
{
    axis0.MoveRelativeAsync(100.0),
    axis1.MoveRelativeAsync(100.0)
};
await Task.WhenAll(tasks);
```

### 3. 错误处理

```csharp
// ✅ 推荐：具体的异常处理
try
{
    await axis.MoveRelativeAsync(100.0);
}
catch (AxisLimitException ex)
{
    // 处理限位
    await HandleLimitException(ex);
}
catch (AxisMotionException ex)
{
    // 处理运动异常
    await HandleMotionException(ex);
}

// ❌ 不推荐：捕获所有异常
try
{
    await axis.MoveRelativeAsync(100.0);
}
catch (Exception ex)
{
    // 无法区分异常类型
}
```

### 4. 日志记录

```csharp
// ✅ 推荐：结构化日志
_logger.LogInformation(
    "轴 {AxisNo} 移动到位置 {Position}，耗时 {Duration}ms",
    axisNo, position, duration);

// ❌ 不推荐：字符串拼接
_logger.LogInformation(
    $"轴 {axisNo} 移动到位置 {position}，耗时 {duration}ms");
```

### 5. 参数验证

```csharp
public async Task<bool> MoveRelativeAsync(double distance)
{
    // ✅ 推荐：参数验证
    if (double.IsNaN(distance) || double.IsInfinity(distance))
    {
        throw new ArgumentException("距离参数无效", nameof(distance));
    }
    
    if (Math.Abs(distance) > MaxDistance)
    {
        throw new ArgumentOutOfRangeException(
            nameof(distance), 
            "距离超出允许范围");
    }
    
    // 执行运动
    return await ExecuteMoveAsync(distance);
}
```

### 6. 状态检查

```csharp
// ✅ 推荐：运动前检查状态
public async Task<bool> MoveRelativeAsync(double distance)
{
    var status = await GetStatusAsync();
    
    if (status.Alarm)
    {
        throw new AxisException("轴处于报警状态，无法运动");
    }
    
    if (status.State == AxisState.Moving)
    {
        throw new AxisException("轴正在运动中");
    }
    
    // 执行运动
    return await ExecuteMoveAsync(distance);
}
```

### 7. 配置管理

```csharp
// ✅ 推荐：使用选项模式
public class AxisService
{
    private readonly IOptions<AxisConfiguration> _config;
    
    public AxisService(IOptions<AxisConfiguration> config)
    {
        _config = config;
    }
}

// ❌ 不推荐：硬编码配置
public class AxisService
{
    private const double MaxSpeed = 100.0;
    private const double Acceleration = 500.0;
}
```

### 8. 依赖注入

```csharp
// ✅ 推荐：构造函数注入
public class MotionControlService
{
    private readonly IMotionCard _motionCard;
    private readonly ILogger _logger;
    
    public MotionControlService(
        IMotionCard motionCard,
        ILogger<MotionControlService> logger)
    {
        _motionCard = motionCard;
        _logger = logger;
    }
}

// ❌ 不推荐：服务定位器
public class MotionControlService
{
    private readonly IServiceProvider _serviceProvider;
    
    public void DoSomething()
    {
        var motionCard = _serviceProvider.GetService<IMotionCard>();
    }
}
```

### 9. 线程安全

```csharp
// ✅ 推荐：使用锁保护共享资源
private readonly SemaphoreSlim _lock = new(1, 1);

public async Task<bool> MoveRelativeAsync(double distance)
{
    await _lock.WaitAsync();
    try
    {
        // 执行运动
        return await ExecuteMoveAsync(distance);
    }
    finally
    {
        _lock.Release();
    }
}
```

### 10. 性能优化

```csharp
// ✅ 推荐：批量操作
var values = await io.ReadInputBitsAsync(0, 16);

// ❌ 不推荐：循环单个操作
var values = new bool[16];
for (int i = 0; i < 16; i++)
{
    values[i] = await io.ReadInputBitAsync((ushort)i);
}

// ✅ 推荐：缓存不变的数据
private CardInfo _cachedCardInfo;
public CardInfo GetCardInfo()
{
    return _cachedCardInfo ??= QueryCardInfo();
}
```

---

## 📚 常用枚举值

### AxisState（轴状态）
```csharp
public enum AxisState
{
    Idle = 0,           // 空闲
    Moving = 1,         // 运动中
    Stopping = 2,       // 停止中
    Error = 3           // 错误
}
```

### StopMode（停止模式）
```csharp
public enum StopMode
{
    Immediate = 0,      // 立即停止
    Deceleration = 1,   // 减速停止
    Emergency = 2       // 急停
}
```

### MotionMode（运动模式）
```csharp
public enum MotionMode
{
    Relative = 0,       // 相对运动
    Absolute = 1        // 绝对运动
}
```

### HomeMode（回零模式）
```csharp
public enum HomeMode
{
    HomeSwitch = 0,     // 原点开关
    Index = 1,          // 编码器Z相
    Limit = 2           // 限位开关
}
```

---

## 🎯 性能指标参考

| 操作 | 响应时间 | 说明 |
|------|----------|------|
| 板卡初始化 | < 100ms | 首次初始化 |
| 单轴运动命令 | < 10ms | 发送命令到板卡 |
| IO读取 | < 5ms | 单个IO位 |
| IO批量读取 | < 10ms | 16个IO位 |
| 位置读取 | < 5ms | 读取当前位置 |
| 状态查询 | < 10ms | 完整状态信息 |

---

## 📞 技术支持

- 项目文档：`/plans/architecture-design.md`
- 实施计划：`/plans/implementation-plan.md`
- 问题反馈：创建 GitHub Issue
- 技术讨论：参与 GitHub Discussions

---

## 🔄 版本历史

- **v1.0.0** - 基础功能实现
  - 板卡初始化和管理
  - 单轴运动控制
  - 基本IO控制
  - 日志和异常处理

---

## 📖 相关资源

- [雷赛官方文档](https://www.leadshine.com)
- [.NET依赖注入文档](https://docs.microsoft.com/dotnet/core/extensions/dependency-injection)
- [Microsoft.Extensions.Logging文档](https://docs.microsoft.com/dotnet/core/extensions/logging)
- [异步编程最佳实践](https://docs.microsoft.com/dotnet/csharp/async)

---

**提示**：本指南会随着项目发展持续更新，建议收藏以便随时查阅。
