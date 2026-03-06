namespace LeadshineCard.Core.Interfaces;

/// <summary>
/// 插补控制器接口
/// </summary>
public interface IInterpolationController
{
    /// <summary>
    /// 直线插补
    /// </summary>
    /// <param name="axes">轴号数组</param>
    /// <param name="targetPositions">目标位置数组 (单位: mm 或 度)</param>
    /// <returns>是否成功</returns>
    Task<bool> LineInterpolationAsync(ushort[] axes, double[] targetPositions);

    /// <summary>
    /// 圆弧插补（圆心终点式）
    /// </summary>
    /// <param name="axes">轴号数组（2轴或3轴）</param>
    /// <param name="targetPositions">目标位置数组</param>
    /// <param name="centerPositions">圆心位置数组</param>
    /// <param name="clockwise">是否顺时针</param>
    /// <returns>是否成功</returns>
    Task<bool> ArcInterpolationAsync(ushort[] axes, double[] targetPositions, 
                                      double[] centerPositions, bool clockwise);

    /// <summary>
    /// 打开连续插补缓冲区
    /// </summary>
    /// <param name="crd">坐标系号</param>
    /// <param name="axes">参与插补的轴号数组</param>
    /// <returns>是否成功</returns>
    Task<bool> OpenContinuousBufferAsync(ushort crd, ushort[] axes);

    /// <summary>
    /// 关闭连续插补缓冲区
    /// </summary>
    /// <param name="crd">坐标系号</param>
    /// <returns>是否成功</returns>
    Task<bool> CloseContinuousBufferAsync(ushort crd);

    /// <summary>
    /// 添加直线段到连续插补缓冲区
    /// </summary>
    /// <param name="crd">坐标系号</param>
    /// <param name="targetPositions">目标位置数组</param>
    /// <param name="mark">段标号</param>
    /// <returns>是否成功</returns>
    Task<bool> AddLineSegmentAsync(ushort crd, double[] targetPositions, int mark = 0);

    /// <summary>
    /// 添加圆弧段到连续插补缓冲区
    /// </summary>
    /// <param name="crd">坐标系号</param>
    /// <param name="targetPositions">目标位置数组</param>
    /// <param name="centerPositions">圆心位置数组</param>
    /// <param name="clockwise">是否顺时针</param>
    /// <param name="mark">段标号</param>
    /// <returns>是否成功</returns>
    Task<bool> AddArcSegmentAsync(ushort crd, double[] targetPositions, 
                                   double[] centerPositions, bool clockwise, int mark = 0);

    /// <summary>
    /// 开始连续插补
    /// </summary>
    /// <param name="crd">坐标系号</param>
    /// <returns>是否成功</returns>
    Task<bool> StartContinuousAsync(ushort crd);

    /// <summary>
    /// 暂停连续插补
    /// </summary>
    /// <param name="crd">坐标系号</param>
    /// <returns>是否成功</returns>
    Task<bool> PauseContinuousAsync(ushort crd);

    /// <summary>
    /// 停止连续插补
    /// </summary>
    /// <param name="crd">坐标系号</param>
    /// <returns>是否成功</returns>
    Task<bool> StopContinuousAsync(ushort crd);

    /// <summary>
    /// 获取剩余缓冲区空间
    /// </summary>
    /// <param name="crd">坐标系号</param>
    /// <returns>剩余空间数量</returns>
    Task<int> GetRemainingBufferSpaceAsync(ushort crd);

    /// <summary>
    /// 获取当前段标号
    /// </summary>
    /// <param name="crd">坐标系号</param>
    /// <returns>当前段标号</returns>
    Task<int> GetCurrentMarkAsync(ushort crd);

    /// <summary>
    /// 检查连续插补是否完成
    /// </summary>
    /// <param name="crd">坐标系号</param>
    /// <returns>是否完成</returns>
    Task<bool> CheckContinuousDoneAsync(ushort crd);
}
