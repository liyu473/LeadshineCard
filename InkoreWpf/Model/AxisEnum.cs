using System.ComponentModel;

namespace InkoreWpf.Model;

/// <summary>
/// 轴名称枚举。
/// </summary>
public enum AxisEnum : uint
{
    /// <summary>
    /// X 轴
    /// </summary>
    [Description("X 轴")]
    X = 0,

    /// <summary>
    /// Y 轴
    /// </summary>
    [Description("Y 轴")]
    Y = 1,

    /// <summary>
    /// Z 轴
    /// </summary>
    [Description("Z 轴")]
    Z = 2,

    /// <summary>
    /// A 轴
    /// </summary>
    [Description("A 轴")]
    A = 3,

    /// <summary>
    /// B 轴
    /// </summary>
    [Description("B 轴")]
    B = 4,

    /// <summary>
    /// C 轴
    /// </summary>
    [Description("C 轴")]
    C = 5,

    /// <summary>
    /// U 轴
    /// </summary>
    [Description("U 轴")]
    U = 6,

    /// <summary>
    /// V 轴
    /// </summary>
    [Description("V 轴")]
    V = 7,

    /// <summary>
    /// W 轴
    /// </summary>
    [Description("W 轴")]
    W = 8,

    /// <summary>
    /// D 轴
    /// </summary>
    [Description("D 轴")]
    D = 9,

    /// <summary>
    /// E 轴
    /// </summary>
    [Description("E 轴")]
    E = 10,

    /// <summary>
    /// F 轴
    /// </summary>
    [Description("F 轴")]
    F = 11
}
