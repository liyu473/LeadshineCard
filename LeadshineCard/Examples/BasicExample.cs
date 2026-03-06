using LeadshineCard.Core.Enums;
using LeadshineCard.Core.Interfaces;
using LeadshineCard.Core.Models;
using LeadshineCard.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LeadshineCard.Examples;

/// <summary>
/// 基础使用示例
/// </summary>
public class BasicExample
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("=== 雷赛运动控制卡示例程序 ===\n");

        // 1. 配置依赖注入
        var services = new ServiceCollection();
        
        services.AddLeadshineMotionControl(logging =>
        {
            logging.AddConsole();
            logging.SetMinimumLevel(LogLevel.Information);
        });

        var serviceProvider = services.BuildServiceProvider();

        // 2. 获取板卡实例
        var motionCard = serviceProvider.GetRequiredService<IMotionCard>();

        try
        {
            // 3. 初始化板卡
            Console.WriteLine("正在初始化板卡...");
            var initResult = await motionCard.InitializeAsync(0);
            
            if (!initResult)
            {
                Console.WriteLine("板卡初始化失败！");
                return;
            }

            Console.WriteLine("板卡初始化成功！\n");

            // 4. 获取板卡信息
            var cardInfo = motionCard.GetCardInfo();
            Console.WriteLine($"板卡信息:");
            Console.WriteLine($"  板卡号: {cardInfo.CardNo}");
            Console.WriteLine($"  总轴数: {cardInfo.TotalAxes}");
            Console.WriteLine($"  输入IO: {cardInfo.TotalInputs}");
            Console.WriteLine($"  输出IO: {cardInfo.TotalOutputs}");
            Console.WriteLine($"  固件版本: {cardInfo.FirmwareVersion}\n");

            // 5. 单轴运动示例
            await SingleAxisMotionExample(motionCard);

            // 6. IO控制示例
            await IoControlExample(motionCard);

            // 7. 插补运动示例
            await InterpolationExample(motionCard);

            Console.WriteLine("\n所有示例执行完成！");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"发生错误: {ex.Message}");
            Console.WriteLine($"详细信息: {ex}");
        }
        finally
        {
            // 8. 关闭板卡
            Console.WriteLine("\n正在关闭板卡...");
            await motionCard.CloseAsync();
            Console.WriteLine("板卡已关闭");
        }

        Console.WriteLine("\n按任意键退出...");
        Console.ReadKey();
    }

    /// <summary>
    /// 单轴运动示例
    /// </summary>
    private static async Task SingleAxisMotionExample(IMotionCard motionCard)
    {
        Console.WriteLine("=== 单轴运动示例 ===");

        // 获取轴0控制器
        var axis = motionCard.GetAxisController(0);

        // 设置运动参数
        var parameters = new MotionParameters
        {
            MaxSpeed = 50.0,        // 最大速度 50 mm/s
            Acceleration = 200.0,   // 加速度 200 mm/s²
            Deceleration = 200.0,   // 减速度 200 mm/s²
            PulseEquivalent = 0.001 // 脉冲当量 0.001 mm/脉冲
        };

        Console.WriteLine("设置运动参数...");
        await axis.SetMotionParametersAsync(parameters);

        // 相对运动
        Console.WriteLine("执行相对运动 +100mm...");
        await axis.MoveRelativeAsync(100.0);

        // 等待运动完成
        Console.WriteLine("等待运动完成...");
        while (!await axis.CheckDoneAsync())
        {
            var status = await axis.GetStatusAsync();
            Console.WriteLine($"  位置: {status.Position:F3} mm, 速度: {status.Speed:F2} mm/s");
            await Task.Delay(100);
        }

        var finalPos = await axis.GetCurrentPositionAsync();
        Console.WriteLine($"运动完成，当前位置: {finalPos:F3} mm\n");
    }

    /// <summary>
    /// IO控制示例
    /// </summary>
    private static async Task IoControlExample(IMotionCard motionCard)
    {
        Console.WriteLine("=== IO控制示例 ===");

        var io = motionCard.GetIoController();

        // 读取输入
        Console.WriteLine("读取输入IO 0-7:");
        var inputs = await io.ReadInputBitsAsync(0, 8);
        for (int i = 0; i < inputs.Length; i++)
        {
            Console.WriteLine($"  IN{i}: {(inputs[i] ? "ON" : "OFF")}");
        }

        // 写入输出
        Console.WriteLine("\n控制输出IO:");
        Console.WriteLine("  设置 OUT0 = ON");
        await io.WriteOutputBitAsync(0, true);
        await Task.Delay(500);

        Console.WriteLine("  设置 OUT0 = OFF");
        await io.WriteOutputBitAsync(0, false);
        Console.WriteLine();
    }

    /// <summary>
    /// 插补运动示例
    /// </summary>
    private static async Task InterpolationExample(IMotionCard motionCard)
    {
        Console.WriteLine("=== 插补运动示例 ===");

        var interpolation = motionCard.GetInterpolationController();

        // 直线插补
        Console.WriteLine("执行直线插补 (X=100, Y=100)...");
        var axes = new ushort[] { 0, 1 };
        var targetPos = new double[] { 100.0, 100.0 };
        
        await interpolation.LineInterpolationAsync(axes, targetPos);

        // 等待插补完成
        await Task.Delay(2000);
        Console.WriteLine("直线插补完成\n");
    }
}
