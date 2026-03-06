# 雷赛运动控制卡 C# 封装库

一个优雅、现代化的雷赛运动控制卡C#封装库，采用依赖注入、接口抽象和异步编程设计。

## ✨ 特性

- 🎯 **接口抽象** - 面向接口编程，易于扩展和测试
- 💉 **依赖注入** - 使用Microsoft.Extensions.DependencyInjection
- 📝 **完善的日志** - 集成Microsoft.Extensions.Logging
- ⚡ **异步编程** - 所有IO操作使用async/await
- 🛡️ **异常处理** - 分层异常体系，错误信息清晰
- 📖 **XML注释** - 所有公共API都有完整的XML文档注释

## 📦 项目结构

```
LeadshineCard/
├── Core/                           # 核心抽象层
│   ├── Interfaces/                 # 接口定义
│   ├── Models/                     # 数据模型
│   ├── Enums/                      # 枚举定义
│   └── Exceptions/                 # 异常定义
├── Implementation/                 # 实现层
│   ├── LeadshineMotionCard.cs     # 板卡实现
│   ├── LeadshineAxisController.cs # 轴控制实现
│   ├── LeadshineIoController.cs   # IO控制实现
│   └── LeadshineInterpolationController.cs # 插补实现
├── Extensions/                     # 扩展方法
│   └── ServiceCollectionExtensions.cs
├── Examples/                       # 示例程序
│   └── BasicExample.cs
└── ThirdPart/                      # 第三方DLL封装
    └── LTDMC.cs
```

## 🚀 快速开始

### 1. 安装依赖

```bash
dotnet add package Microsoft.Extensions.DependencyInjection
dotnet add package Microsoft.Extensions.Logging
dotnet add package Microsoft.Extensions.Logging.Console
```

### 2. 配置服务

```csharp
using LeadshineCard.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var services = new ServiceCollection();

services.AddLeadshineMotionControl(logging =>
{
    logging.AddConsole();
    logging.SetMinimumLevel(LogLevel.Information);
});

var serviceProvider = services.BuildServiceProvider();
```

### 3. 使用板卡

```csharp
using LeadshineCard.Core.Interfaces;

// 获取板卡实例
var motionCard = serviceProvider.GetRequiredService<IMotionCard>();

// 初始化板卡
await motionCard.InitializeAsync(0);

// 获取板卡信息
var cardInfo = motionCard.GetCardInfo();
Console.WriteLine($"总轴数: {cardInfo.TotalAxes}");
```

## 📚 使用示例

### 单轴运动

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

// 等待运动完成
while (!await axis.CheckDoneAsync())
{
    await Task.Delay(10);
}

// 读取位置
var position = await axis.GetCurrentPositionAsync();
```

### IO控制

```csharp
var io = motionCard.GetIoController();

// 读取输入
var input = await io.ReadInputBitAsync(0);

// 写入输出
await io.WriteOutputBitAsync(0, true);

// 批量读取
var inputs = await io.ReadInputBitsAsync(0, 8);
```

### 直线插补

```csharp
var interpolation = motionCard.GetInterpolationController();

// 两轴直线插补
await interpolation.LineInterpolationAsync(
    new ushort[] { 0, 1 },
    new double[] { 100.0, 100.0 }
);
```

### 连续插补

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

// 开始插补
await interpolation.StartContinuousAsync(crd);
```

## 🏗️ 架构设计

### 核心接口

- [`IMotionCard`](Core/Interfaces/IMotionCard.cs) - 板卡管理接口
- [`IAxisController`](Core/Interfaces/IAxisController.cs) - 轴控制接口
- [`IIoController`](Core/Interfaces/IIoController.cs) - IO控制接口
- [`IInterpolationController`](Core/Interfaces/IInterpolationController.cs) - 插补控制接口

### 实现类

- [`LeadshineMotionCard`](Implementation/LeadshineMotionCard.cs) - 雷赛板卡实现
- [`LeadshineAxisController`](Implementation/LeadshineAxisController.cs) - 雷赛轴控制实现
- [`LeadshineIoController`](Implementation/LeadshineIoController.cs) - 雷赛IO控制实现
- [`LeadshineInterpolationController`](Implementation/LeadshineInterpolationController.cs) - 雷赛插补实现

## 📖 文档

- [架构设计文档](plans/architecture-design.md) - 完整的架构设计说明
- [实施计划](plans/implementation-plan.md) - 详细的实施计划和Mermaid图
- [快速参考指南](plans/quick-reference.md) - 常用代码片段和最佳实践

## 🔧 开发环境

- .NET 10.0
- C# 12.0
- Visual Studio 2022 或 VS Code

## 📝 许可证

本项目采用 MIT 许可证。

## 🤝 贡献

欢迎提交 Issue 和 Pull Request！

## 📞 联系方式

如有问题，请创建 Issue 或查看文档。

---

**注意**: 使用前请确保已正确安装雷赛板卡驱动和LTDMC.dll文件。
