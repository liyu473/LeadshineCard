using KingGleeVision.Models;
using OpenCvSharp;

namespace KingGleeVision;

public static class FindMark
{
    /// <summary>
    /// 将图像从中间垂直分割成左右两部分（不携带原图坐标信息）
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

    /// <summary>
    /// 将裁剪结果从中间垂直分割成左右两部分，并保留各部分在原图中的坐标信息
    /// </summary>
    /// <param name="cropResult">裁剪结果（包含裁剪图和在原图中的位置）</param>
    /// <returns>左半部分和右半部分的 CropResult 元组，每个都包含在原图中的坐标</returns>
    public static (CropResult left, CropResult right) SplitCropResultVertically(this CropResult cropResult)
    {
        var croppedMat = cropResult.CroppedMat;
        var roiInOriginal = cropResult.RoiInOriginal;

        int width = croppedMat.Width;
        int height = croppedMat.Height;
        int halfWidth = width / 2;

        // 左半部分：在裁剪图中的 ROI
        Rect leftLocalRoi = new Rect(0, 0, halfWidth, height);
        Mat leftMat = new Mat(croppedMat, leftLocalRoi);
        // 左半部分在原图中的位置：X 不变，宽度为 halfWidth
        Rect leftOriginalRoi = new Rect(
            roiInOriginal.X,
            roiInOriginal.Y,
            halfWidth,
            height
        );
        var leftResult = new CropResult(leftMat, leftOriginalRoi);

        // 右半部分：在裁剪图中的 ROI
        Rect rightLocalRoi = new Rect(halfWidth, 0, width - halfWidth, height);
        Mat rightMat = new Mat(croppedMat, rightLocalRoi);
        // 右半部分在原图中的位置：X 偏移 halfWidth
        Rect rightOriginalRoi = new Rect(
            roiInOriginal.X + halfWidth,
            roiInOriginal.Y,
            width - halfWidth,
            height
        );
        var rightResult = new CropResult(rightMat, rightOriginalRoi);

        return (leftResult, rightResult);
    }
}
