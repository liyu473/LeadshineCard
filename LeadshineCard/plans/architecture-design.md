# 雷赛运动控制卡架构设计方案

## 一、项目概述

本项目旨在为雷赛运动控制卡提供一个优雅、可扩展、易于维护的C#封装库，支持多种工控应用场景。

### 设计原则
- **依赖注入（DI）**：使用Microsoft.Extensions.DependencyInjection实现松耦合
- **接口抽象**：面向接口编程，便于扩展和测试
- **日志系统**：集成Microsoft.Extensions.Logging，支持多种日志输出
- **配置管理**：使用Microsoft.Extensions.Configuration管理参数
- **异常处理**：统一的异常处理机制
- **单一职责**：每个类只负责一个功能模块

---

## 二、架构分层设计

```
┌─────────────────────────────────────────────────┐
│           应用层 (Application Layer)              │
│  - 控制台应用 / WPF / WinForms / Web API        │
└─────────────────────────────────────────────────┘
                      ↓
┌─────────────────────────────────────────────────┐
│           服务层 (Service Layer)                 │
│  - MotionControlService                         │
│  - AxisService                                  │
│  - IoService                                    │
│  - InterpolationService                         │
└─────────────────────────────────────────────────┘
                      ↓
┌─────────────────────────────────────────────────┐
│         核心抽象层 (Core Abstraction)            │
│  - IMotionCard (板卡接口)                       │
│  - IAxisController (轴控制接口)                 │
│  - IIoController (IO控制接口)                   │
│  - IInterpolationController (插补接口)          │
└─────────────────────────────────────────────────┘
                      ↓
┌─────────────────────────────────────────────────┐
│         实现层 (Implementation Layer)            │
│  - LeadshineMotionCard                          │
│  - LeadshineAxisController                      │
│  - LeadshineIoController                        │
│  - LeadshineInterpolationController             │
└─────────────────────────────────────────────────┘
                      ↓
┌─────────────────────────────────────────────────┐
│         硬件层 (Hardware Layer)                  │
│  - LTDMC.cs (DLL封装)                           │
└─────────────────────────────────────────────────┘
```

---

## 三、核心模块划分

### 3.1 板卡管理模块 (Card Management)
- 板卡初始化/关闭
- 板卡信息查询
- 多板卡管理
- 连接状态监控

### 3.2 轴运动控制模块 (Axis Motion Control)
- 单轴点位运动（相对/绝对）
- JOG运动
- 回零运动
- 速度控制
- 位置读取
- 运动状态查询

### 3.3 IO控制模块 (IO Control)
- 通用IO读写
- 专用IO配置
- IO映射管理
- IO状态监控

### 3.4 插补运动模块 (Interpolation)
- 直线插补
- 圆弧插补
- 连续插补
- 轨迹规划

### 3.5 编码器模块 (Encoder)
- 编码器读取
- 编码器配置
- 位置锁存

### 3.6 配置管理模块 (Configuration)
- 轴参数配置
- 板卡参数配置
- 配置文件加载/保存

### 3.7 日志模块 (Logging)
- 操作日志
- 错误日志
- 性能日志
- 调试日志

### 3.8 异常处理模块 (Exception Handling)
- 统一异常定义
- 错误码映射
- 异常恢复机制

---

## 四、项目目录结构

```
LeadshineCard/
├── ThirdPart/                          # 第三方DLL封装
│   └── LTDMC.cs                        # 雷赛DLL导入
│
├── Core/                               # 核心抽象层
│   ├── Interfaces/                     # 接口定义
│   │   ├── IMotionCard.cs             # 板卡接口
│   │   ├── IAxisController.cs         # 轴控制接口
│   │   ├── IIoController.cs           # IO控制接口
│   │   ├── IInterpolationController.cs # 插补接口
│   │   ├── IEncoderController.cs      # 编码器接口
│   │   └── IHomeController.cs         # 回零接口
│   │
│   ├── Models/                         # 数据模型
│   │   ├── AxisInfo.cs                # 轴信息
│   │   ├── CardInfo.cs                # 板卡信息
│   │   ├── MotionParameters.cs        # 运动参数
│   │   ├── AxisStatus.cs              # 轴状态
│   │   └── IoStatus.cs                # IO状态
│   │
│   ├── Enums/                          # 枚举定义
│   │   ├── AxisState.cs               # 轴状态枚举
│   │   ├── MotionMode.cs              # 运动模式
│   │   ├── StopMode.cs                # 停止模式
│   │   └── ErrorCode.cs               # 错误码
│   │
│   └── Exceptions/                     # 异常定义
│       ├── MotionCardException.cs     # 板卡异常
│       ├── AxisException.cs           # 轴异常
│       └── IoException.cs             # IO异常
│
├── Implementation/                     # 实现层
│   ├── LeadshineMotionCard.cs         # 雷赛板卡实现
│   ├── LeadshineAxisController.cs     # 雷赛轴控制实现
│   ├── LeadshineIoController.cs       # 雷赛IO控制实现
│   ├── LeadshineInterpolationController.cs # 雷赛插补实现
│   ├── LeadshineEncoderController.cs  # 雷赛编码器实现
│   └── LeadshineHomeController.cs     # 雷赛回零实现
│
├── Services/                           # 服务层
│   ├── MotionControlService.cs        # 运动控制服务
│   ├── AxisService.cs                 # 轴服务
│   ├── IoService.cs                   # IO服务
│   └── ConfigurationService.cs        # 配置服务
│
├── Configuration/                      # 配置管理
│   ├── CardConfiguration.cs           # 板卡配置
│   ├── AxisConfiguration.cs           # 轴配置
│   └── appsettings.json               # 配置文件
│
├── Extensions/                         # 扩展方法
│   ├── ServiceCollectionExtensions.cs # DI扩展
│   └── ErrorCodeExtensions.cs         # 错误码扩展
│
└── Utils/                              # 工具类
    ├── ErrorCodeMapper.cs             # 错误码映射
    └── MotionHelper.cs                # 运动辅助工具
```

---

## 五、核心接口设计

### 5.1 IMotionCard - 板卡接口

```csharp
public interface IMotionCard : IDisposable
{
    // 板卡初始化
    Task<bool> InitializeAsync(ushort cardNo);
    
    // 板卡关闭
    Task<bool> CloseAsync();
    
    // 获取板卡信息
    CardInfo GetCardInfo();
    
    // 获取轴控制器
    IAxisController GetAxisController(ushort axisNo);
    
    // 获取IO控制器
    IIoController GetIoController();
    
    // 获取插补控制器
    IInterpolationController GetInterpolationController();
    
    // 板卡复位
    Task<bool> ResetAsync();
    
    // 检查连接状态
    bool IsConnected { get; }
}
```

### 5.2 IAxisController - 轴控制接口

```csharp
public interface IAxisController
{
    // 轴号
    ushort AxisNo { get; }
    
    // 设置运动参数
    Task SetMotionParametersAsync(MotionParameters parameters);
    
    // 相对运动
    Task<bool> MoveRelativeAsync(double distance);
    
    // 绝对运动
    Task<bool> MoveAbsoluteAsync(double position);
    
    // JOG运动
    Task<bool> JogAsync(bool positiveDirection);
    
    // 停止运动
    Task<bool> StopAsync(StopMode mode);
    
    // 获取当前位置
    Task<double> GetCurrentPositionAsync();
    
    // 获取当前速度
    Task<double> GetCurrentSpeedAsync();
    
    // 获取轴状态
    Task<AxisStatus> GetStatusAsync();
    
    // 检查运动完成
    Task<bool> CheckDoneAsync();
    
    // 回零
    Task<bool> HomeAsync();
}
```

### 5.3 IIoController - IO控制接口

```csharp
public interface IIoController
{
    // 读取输入位
    Task<bool> ReadInputBitAsync(ushort bitNo);
    
    // 写入输出位
    Task<bool> WriteOutputBitAsync(ushort bitNo, bool value);
    
    // 读取输入端口
    Task<uint> ReadInputPortAsync(ushort portNo);
    
    // 写入输出端口
    Task<bool> WriteOutputPortAsync(ushort portNo, uint value);
    
    // 批量读取输入
    Task<bool[]> ReadInputBitsAsync(ushort startBit, ushort count);
    
    // 批量写入输出
    Task<bool> WriteOutputBitsAsync(ushort startBit, bool[] values);
}
```

### 5.4 IInterpolationController - 插补控制接口

```csharp
public interface IInterpolationController
{
    // 直线插补
    Task<bool> LineInterpolationAsync(ushort[] axes, double[] targetPositions);
    
    // 圆弧插补
    Task<bool> ArcInterpolationAsync(ushort[] axes, double[] targetPos, 
                                      double[] centerPos, bool clockwise);
    
    // 打开连续插补缓冲区
    Task<bool> OpenContinuousBufferAsync(ushort crd, ushort[] axes);
    
    // 添加直线段
    Task<bool> AddLineSegmentAsync(ushort crd, double[] targetPos);
    
    // 添加圆弧段
    Task<bool> AddArcSegmentAsync(ushort crd, double[] targetPos, 
                                   double[] centerPos, bool clockwise);
    
    // 开始连续插补
    Task<bool> StartContinuousAsync(ushort crd);
    
    // 暂停连续插补
    Task<bool> PauseContinuousAsync(ushort crd);
    
    // 停止连续插补
    Task<bool> StopContinuousAsync(ushort crd);
    
    // 获取剩余缓冲区空间
    Task<int> GetRemainingBufferSpaceAsync(ushort crd);
}
```

---

## 六、依赖注入配置

### 6.1 服务注册

```csharp
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddLeadshineMotionControl(
        this IServiceCollection services, 
        Action<CardConfiguration> configureOptions = null)
    {
        // 配置选项
        if (configureOptions != null)
        {
            services.Configure(configureOptions);
        }
        
        // 注册核心服务
        services.AddSingleton<IMotionCard, LeadshineMotionCard>();
        services.AddTransient<IAxisController, LeadshineAxisController>();
        services.AddTransient<IIoController, LeadshineIoController>();
        services.AddTransient<IInterpolationController, LeadshineInterpolationController>();
        
        // 注册业务服务
        services.AddScoped<MotionControlService>();
        services.AddScoped<AxisService>();
        services.AddScoped<IoService>();
        
        // 注册日志
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.AddDebug();
            builder.AddFile("logs/motion-{Date}.log");
        });
        
        return services;
    }
}
```

### 6.2 使用示例

```csharp
// Program.cs
var builder = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddLeadshineMotionControl(options =>
        {
            options.CardNo = 0;
            options.MaxAxes = 4;
            options.EnableAutoReconnect = true;
        });
    });

var host = builder.Build();

// 使用服务
var motionService = host.Services.GetRequiredService<MotionControlService>();
await motionService.InitializeAsync();
```

---

## 七、日志系统设计

### 7.1 日志级别使用规范

- **Trace**: 详细的调试信息（如每次DLL调用）
- **Debug**: 调试信息（如参数设置）
- **Information**: 一般信息（如板卡初始化成功）
- **Warning**: 警告信息（如轴接近限位）
- **Error**: 错误信息（如运动失败）
- **Critical**: 严重错误（如板卡断开连接）

### 7.2 日志示例

```csharp
public class LeadshineAxisController : IAxisController
{
    private readonly ILogger<LeadshineAxisController> _logger;
    
    public async Task<bool> MoveRelativeAsync(double distance)
    {
        _logger.LogInformation(
            "轴 {AxisNo} 开始相对运动，距离: {Distance}", 
            AxisNo, distance);
        
        try
        {
            var result = LTDMC.dmc_pmove_unit(_cardNo, AxisNo, distance, 1);
            
            if (result != 0)
            {
                _logger.LogError(
                    "轴 {AxisNo} 相对运动失败，错误码: {ErrorCode}", 
                    AxisNo, result);
                return false;
            }
            
            _logger.LogDebug(
                "轴 {AxisNo} 相对运动命令发送成功", 
                AxisNo);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "轴 {AxisNo} 相对运动异常", 
                AxisNo);
            throw;
        }
    }
}
```

---

## 八、配置管理

### 8.1 配置文件结构 (appsettings.json)

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
        "SoftLimitNegative": -1000.0
      },
      {
        "AxisNo": 1,
        "Name": "Y轴",
        "MaxSpeed": 100.0,
        "Acceleration": 500.0,
        "Deceleration": 500.0,
        "PulseEquivalent": 0.001,
        "SoftLimitPositive": 1000.0,
        "SoftLimitNegative": -1000.0
      }
    ],
    "Logging": {
      "LogLevel": {
        "Default": "Information",
        "LeadshineCard": "Debug"
      },
      "EnableFileLogging": true,
      "LogFilePath": "logs/motion-{Date}.log"
    }
  }
}
```

---

## 九、异常处理机制

### 9.1 异常层次结构

```
MotionCardException (基类)
├── CardInitializationException (板卡初始化异常)
├── CardConnectionException (板卡连接异常)
├── AxisException (轴异常)
│   ├── AxisMotionException (轴运动异常)
│   ├── AxisLimitException (轴限位异常)
│   └── AxisHomeException (轴回零异常)
├── IoException (IO异常)
└── InterpolationException (插补异常)
```

### 9.2 错误码映射

```csharp
public static class ErrorCodeMapper
{
    private static readonly Dictionary<short, string> ErrorMessages = new()
    {
        { 0, "操作成功" },
        { -1, "板卡未初始化" },
        { -2, "板卡号错误" },
        { -3, "轴号错误" },
        { -4, "参数错误" },
        // ... 更多错误码
    };
    
    public static string GetErrorMessage(short errorCode)
    {
        return ErrorMessages.TryGetValue(errorCode, out var message) 
            ? message 
            : $"未知错误码: {errorCode}";
    }
}
```

---

## 十、使用示例

### 10.1 基本使用

```csharp
// 初始化
var motionCard = serviceProvider.GetRequiredService<IMotionCard>();
await motionCard.InitializeAsync(0);

// 获取轴控制器
var axis0 = motionCard.GetAxisController(0);

// 设置运动参数
await axis0.SetMotionParametersAsync(new MotionParameters
{
    MaxSpeed = 100.0,
    Acceleration = 500.0,
    Deceleration = 500.0
});

// 相对运动
await axis0.MoveRelativeAsync(50.0);

// 等待运动完成
while (!await axis0.CheckDoneAsync())
{
    await Task.Delay(10);
}

// 读取位置
var position = await axis0.GetCurrentPositionAsync();
Console.WriteLine($"当前位置: {position}");
```

### 10.2 高级使用 - 连续插补

```csharp
var interpolation = motionCard.GetInterpolationController();

// 打开连续插补缓冲区
await interpolation.OpenContinuousBufferAsync(0, new ushort[] { 0, 1 });

// 添加轨迹段
await interpolation.AddLineSegmentAsync(0, new double[] { 100, 100 });
await interpolation.AddArcSegmentAsync(0, 
    new double[] { 200, 100 }, 
    new double[] { 150, 100 }, 
    true);
await interpolation.AddLineSegmentAsync(0, new double[] { 200, 0 });

// 开始插补
await interpolation.StartContinuousAsync(0);
```

---

## 十一、扩展性设计

### 11.1 支持其他品牌板卡

通过实现相同的接口，可以轻松支持其他品牌的运动控制卡：

```csharp
// 固高板卡实现
public class GoogolMotionCard : IMotionCard { }

// 雷赛板卡实现
public class LeadshineMotionCard : IMotionCard { }

// 在DI中切换
services.AddSingleton<IMotionCard, GoogolMotionCard>();
// 或
services.AddSingleton<IMotionCard, LeadshineMotionCard>();
```

### 11.2 插件化架构

可以通过插件系统动态加载不同的板卡驱动：

```csharp
public interface IMotionCardPlugin
{
    string Name { get; }
    string Version { get; }
    IMotionCard CreateInstance();
}
```

---

## 十二、测试策略

### 12.1 单元测试

- 使用Moq模拟硬件接口
- 测试业务逻辑
- 测试异常处理

### 12.2 集成测试

- 使用真实硬件测试
- 测试完整工作流程
- 性能测试

### 12.3 测试示例

```csharp
[Fact]
public async Task MoveRelative_ShouldReturnTrue_WhenMotionSucceeds()
{
    // Arrange
    var mockCard = new Mock<IMotionCard>();
    var axis = new LeadshineAxisController(mockCard.Object, 0);
    
    // Act
    var result = await axis.MoveRelativeAsync(100.0);
    
    // Assert
    Assert.True(result);
}
```

---

## 十三、性能优化建议

1. **异步操作**: 所有IO操作使用异步方法
2. **对象池**: 复用频繁创建的对象
3. **批量操作**: 提供批量读写IO的方法
4. **缓存**: 缓存板卡信息和轴状态
5. **线程安全**: 使用锁保护共享资源

---

## 十四、安全性考虑

1. **软限位保护**: 防止轴超出安全范围
2. **急停功能**: 提供紧急停止接口
3. **状态验证**: 运动前检查轴状态
4. **参数校验**: 验证所有输入参数
5. **异常恢复**: 提供异常后的恢复机制

---

## 十五、文档和注释

1. **XML注释**: 所有公共接口添加XML注释
2. **使用示例**: 提供丰富的代码示例
3. **API文档**: 使用DocFX生成API文档
4. **架构图**: 使用Mermaid绘制架构图

---

## 十六、后续扩展方向

1. **可视化界面**: 开发WPF/WinForms控制面板
2. **远程控制**: 支持网络远程控制
3. **数据采集**: 记录运动轨迹和性能数据
4. **AI集成**: 集成机器学习优化运动参数
5. **云平台**: 连接云平台实现远程监控

---

## 十七、开发优先级

### 第一阶段（核心功能）
1. 核心接口定义
2. 板卡初始化和管理
3. 单轴运动控制
4. 基本IO控制
5. 日志和异常处理

### 第二阶段（高级功能）
1. 多轴插补
2. 回零功能
3. 编码器支持
4. 配置管理
5. 依赖注入集成

### 第三阶段（扩展功能）
1. 连续插补
2. 高速位置比较
3. 手轮控制
4. CAN总线扩展
5. 性能优化

---

## 十八、总结

本架构设计方案提供了一个完整、可扩展、易于维护的工控运动板卡控制系统。通过依赖注入、接口抽象和日志系统，实现了高内聚低耦合的设计，便于后续的功能扩展和维护。

关键优势：
- ✅ 面向接口编程，易于测试和扩展
- ✅ 依赖注入，松耦合设计
- ✅ 完善的日志系统
- ✅ 统一的异常处理
- ✅ 灵活的配置管理
- ✅ 支持多种应用场景
- ✅ 良好的代码组织结构
