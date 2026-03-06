using OpenCvSharp;

namespace KingGleeVision.Models;

/// <summary>
/// 裁剪结果，保存裁剪后的图像及其在原图中的位置信息
/// </summary>
public class CropResult
{
    /// <summary>
    /// 裁剪后的图像
    /// </summary>
    public Mat CroppedMat { get; init; }

    /// <summary>
    /// 裁剪区域在原图中的位置（Rect: X, Y, Width, Height）
    /// </summary>
    public Rect RoiInOriginal { get; init; }

    public CropResult(Mat croppedMat, Rect roiInOriginal)
    {
        CroppedMat = croppedMat;
        RoiInOriginal = roiInOriginal;
    }

    /// <summary>
    /// 将裁剪图上的局部坐标转换为原图坐标
    /// </summary>
    /// <param name="localRect">裁剪图上的矩形（例如 ONNX 推理检测到的框）</param>
    /// <returns>在原图上的矩形坐标</returns>
    public Rect ToOriginalCoordinates(Rect localRect)
    {
        return new Rect(
            localRect.X + RoiInOriginal.X,
            localRect.Y + RoiInOriginal.Y,
            localRect.Width,
            localRect.Height
        );
    }

    /// <summary>
    /// 将裁剪图上的局部点坐标转换为原图坐标
    /// </summary>
    /// <param name="localPoint">裁剪图上的点</param>
    /// <returns>在原图上的点坐标</returns>
    public Point ToOriginalCoordinates(Point localPoint)
    {
        return new Point(
            localPoint.X + RoiInOriginal.X,
            localPoint.Y + RoiInOriginal.Y
        );
    }
}
