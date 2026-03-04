using OpenCvSharp;

namespace KingGleeVision;

public static class HoughCircleDetector
{
    /// <summary>
    /// 霍夫圆检测
    /// </summary>
    /// <param name="src"></param>
    /// <param name="dp">累加器分辨率与图像分辨率的反比（默认 1.0）</param>
    /// <param name="minDist">检测到的圆心之间的最小距离（默认 50）</param>
    /// <param name="param1"> Canny 边缘检测的高阈值（默认 100）</param>
    /// <param name="param2">累加器阈值（默认 30）</param>
    /// <param name="minRadius">最小圆半径（默认 0）</param>
    /// <param name="maxRadius">最大圆半径（默认 0，表示不限制）</param>
    /// <returns></returns>
    public static CircleSegment[] DetectCircles(
        Mat src,
        double dp = 1.0,
        double minDist = 50,
        double param1 = 100,
        double param2 = 30,
        int minRadius = 0,
        int maxRadius = 0)
    {
        return Cv2.HoughCircles(
            src,
            HoughModes.Gradient,
            dp,
            minDist,
            param1,
            param2,
            minRadius,
            maxRadius);
    }
}
