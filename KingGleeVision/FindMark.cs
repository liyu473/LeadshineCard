using OpenCvSharp;

namespace KingGleeVision;

public static class FindMark
{
    /// <summary>
    /// 将图像从中间垂直分割成左右两部分
    /// </summary>
    /// <param name="src">源图像</param>
    /// <returns>左半部分和右半部分的元组</returns>
    public static (Mat left, Mat right) SplitMatVertically(this Mat src)
    {
        int width = src.Width;
        int height = src.Height;
        int halfWidth = width / 2;

        Rect leftRoi = new Rect(0, 0, halfWidth, height);
        Mat left = new Mat(src, leftRoi);

        Rect rightRoi = new Rect(halfWidth, 0, width - halfWidth, height);
        Mat right = new Mat(src, rightRoi);

        return (left, right);
    }

    
}
