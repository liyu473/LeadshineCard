using OpenCvSharp;

namespace KingGleeVision.MatPreProcess;

public static class Blur
{
    /// <summary>
    /// 高斯模糊
    /// </summary>
    /// <param name="src">输入图像</param>
    /// <param name="ksize">高斯核大小，必须为正奇数。常用值：(3,3)、(5,5)、(7,7)</param>
    /// <param name="sigmaX">X方向标准差。0表示自动计算。常用值：0、1.0、1.5</param>
    /// <param name="sigmaY">Y方向标准差。0表示与sigmaX相同。常用值：0</param>
    /// <returns>模糊后的图像</returns>
    public static Mat GetGaussianBlur(Mat src, Size ksize, double sigmaX = 0, double sigmaY = 0)
    {
        var dst = new Mat();
        Cv2.GaussianBlur(src, dst, ksize, sigmaX, sigmaY);
        return dst;
    }

    /// <summary>
    /// 高斯模糊（扩展方法）
    /// </summary>
    /// <param name="src">输入图像</param>
    /// <param name="ksize">高斯核大小，必须为正奇数。常用值：(3,3)、(5,5)、(7,7)</param>
    /// <param name="sigmaX">X方向标准差。0表示自动计算。常用值：0、1.0、1.5</param>
    /// <param name="sigmaY">Y方向标准差。0表示与sigmaX相同。常用值：0</param>
    /// <returns>模糊后的图像</returns>
    public static Mat ToGaussianBlur(this Mat src, Size ksize, double sigmaX = 0, double sigmaY = 0)
    {
        var dst = new Mat();
        Cv2.GaussianBlur(src, dst, ksize, sigmaX, sigmaY);
        return dst;
    }

    /// <summary>
    /// 中值滤波（去除椒盐噪声效果好）
    /// </summary>
    /// <param name="src">输入图像</param>
    /// <param name="ksize">核大小，必须为大于1的正奇数。常用值：3、5、7</param>
    /// <returns>滤波后的图像</returns>
    public static Mat GetMedianBlur(Mat src, int ksize)
    {
        var dst = new Mat();
        Cv2.MedianBlur(src, dst, ksize);
        return dst;
    }

    /// <summary>
    /// 中值滤波（扩展方法，去除椒盐噪声效果好）
    /// </summary>
    /// <param name="src">输入图像</param>
    /// <param name="ksize">核大小，必须为大于1的正奇数。常用值：3、5、7</param>
    /// <returns>滤波后的图像</returns>
    public static Mat ToMedianBlur(this Mat src, int ksize)
    {
        var dst = new Mat();
        Cv2.MedianBlur(src, dst, ksize);
        return dst;
    }

    /// <summary>
    /// 均值滤波（简单平滑）
    /// </summary>
    /// <param name="src">输入图像</param>
    /// <param name="ksize">核大小。常用值：(3,3)、(5,5)、(7,7)</param>
    /// <returns>滤波后的图像</returns>
    public static Mat GetBlur(Mat src, Size ksize)
    {
        var dst = new Mat();
        Cv2.Blur(src, dst, ksize);
        return dst;
    }

    /// <summary>
    /// 均值滤波（扩展方法，简单平滑）
    /// </summary>
    /// <param name="src">输入图像</param>
    /// <param name="ksize">核大小。常用值：(3,3)、(5,5)、(7,7)</param>
    /// <returns>滤波后的图像</returns>
    public static Mat ToBlur(this Mat src, Size ksize)
    {
        var dst = new Mat();
        Cv2.Blur(src, dst, ksize);
        return dst;
    }

    /// <summary>
    /// 双边滤波（保边去噪，速度较慢）
    /// </summary>
    /// <param name="src">输入图像</param>
    /// <param name="d">滤波器直径。常用值：5、9。-1表示从sigmaSpace计算</param>
    /// <param name="sigmaColor">颜色空间标准差。值越大，颜色差异越大的像素会被混合。常用值：50、75、100</param>
    /// <param name="sigmaSpace">坐标空间标准差。值越大，距离越远的像素会相互影响。常用值：50、75、100</param>
    /// <returns>滤波后的图像</returns>
    public static Mat GetBilateralFilter(Mat src, int d, double sigmaColor, double sigmaSpace)
    {
        var dst = new Mat();
        Cv2.BilateralFilter(src, dst, d, sigmaColor, sigmaSpace);
        return dst;
    }

    /// <summary>
    /// 双边滤波（扩展方法，保边去噪，速度较慢）
    /// </summary>
    /// <param name="src">输入图像</param>
    /// <param name="d">滤波器直径。常用值：5、9。-1表示从sigmaSpace计算</param>
    /// <param name="sigmaColor">颜色空间标准差。值越大，颜色差异越大的像素会被混合。常用值：50、75、100</param>
    /// <param name="sigmaSpace">坐标空间标准差。值越大，距离越远的像素会相互影响。常用值：50、75、100</param>
    /// <returns>滤波后的图像</returns>
    public static Mat ToBilateralFilter(this Mat src, int d, double sigmaColor, double sigmaSpace)
    {
        var dst = new Mat();
        Cv2.BilateralFilter(src, dst, d, sigmaColor, sigmaSpace);
        return dst;
    }
}
