using KingGleeVision.MatPreProcess;
using KingGleeVision.Models;
using OpenCvSharp;

namespace KingGleeVision;

public static class PcbCropper
{
    public static Mat? PreProcess(Mat src)
    {
        return src.ToGaussianBlur(new Size(3, 3))
            .ToHSV()
            .ToSChannelWithOtsuFromHsv()
            .ToMedianBlur(3);
    }

    /// <summary>
    /// 方法1：实测下来上下有黑边，得配合二次上下黑边处理BorderCropping得到最佳裁剪图
    /// 裁剪 PCB 区域，返回裁剪后的图像和在原图中的位置信息
    /// </summary>
    /// <param name="src">原始图像</param>
    /// <returns>CropResult 包含裁剪后的 Mat 和在原图中的 Rect；若无法裁剪则返回 null</returns>
    public static CropResult? CropPcbArea1(Mat src, bool isBorderCropping = true)
    {
        var pre = PreProcess(src);
        if (pre is null)
            return null;

        Mat binary = new Mat();
        Cv2.Threshold(pre, binary, 200, 255, ThresholdTypes.Binary);

        Mat labels = new Mat();
        Mat stats = new Mat();
        Mat centroids = new Mat();
        int numLabels = Cv2.ConnectedComponentsWithStats(
            binary,
            labels,
            stats,
            centroids,
            PixelConnectivity.Connectivity4
        );
        List<(int area, int x, int y, int w, int h, int idx)> regions = [];
        for (int i = 1; i < numLabels; i++)
        {
            int x = stats.At<int>(i, (int)ConnectedComponentsTypes.Left);
            int y = stats.At<int>(i, (int)ConnectedComponentsTypes.Top);
            int w = stats.At<int>(i, (int)ConnectedComponentsTypes.Width);
            int h = stats.At<int>(i, (int)ConnectedComponentsTypes.Height);
            int area = stats.At<int>(i, (int)ConnectedComponentsTypes.Area);

            regions.Add((area, x, y, w, h, i));
        }

        // 按面积排序（降序）
        regions = [.. regions.OrderByDescending(r => r.area)];

        for (int i = 0; i < Math.Min(10, regions.Count); i++)
        {
            var (area, x, y, w, h, idx) = regions[i];
        }

        if (regions.Count > 0)
        {
            var (bestArea, bestX, bestY, bestW, bestH, bestIdx) = regions[0];

            // 精确裁剪（无边距）
            Rect roiExact = new Rect(bestX, bestY, bestW, bestH);
            Mat croppedExact = new Mat(pre, roiExact);

            var result = new CropResult(croppedExact, roiExact);

            return isBorderCropping ? BorderCropping(result) : result;
        }

        return null;
    }

    /// <summary>
    /// 二次处理裁剪的PCB的黑边
    /// </summary>
    /// <param name="result"></param>
    /// <param name="whiteRatioThreshold">最低白色像素占比</param>
    /// <param name="minContinuousRows">连续命中行数，才认为是PCB区域</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static CropResult? BorderCropping(
        CropResult result,
        double whiteRatioThreshold = 0.4,
        int minContinuousRows = 5
    )
    {
        var src = result.CroppedMat;
        if (src.Empty())
            throw new ArgumentException("Image is empty");

        var gray = src.Channels() == 1 ? src.Clone() : src.CvtColor(ColorConversionCodes.BGR2GRAY);

        Mat binary = new Mat();

        Cv2.Threshold(gray, binary, 0, 255, ThresholdTypes.Binary | ThresholdTypes.Otsu);

        int height = binary.Rows;
        int width = binary.Cols;

        int top = 0;
        int bottom = height - 1;

        // 找上边界
        int run = 0;
        for (int y = 0; y < height; y++)
        {
            int white = Cv2.CountNonZero(binary.Row(y));
            double ratio = (double)white / width;

            if (ratio >= whiteRatioThreshold)
            {
                run++;
                if (run >= minContinuousRows)
                {
                    top = y - minContinuousRows + 1;
                    break;
                }
            }
            else
            {
                run = 0;
            }
        }

        // 找下边界
        run = 0;
        for (int y = height - 1; y >= 0; y--)
        {
            int white = Cv2.CountNonZero(binary.Row(y));
            double ratio = (double)white / width;

            if (ratio >= whiteRatioThreshold)
            {
                run++;
                if (run >= minContinuousRows)
                {
                    bottom = y + minContinuousRows - 1;
                    if (bottom >= height)
                        bottom = height - 1;
                    break;
                }
            }
            else
            {
                run = 0;
            }
        }

        Rect roi = new Rect(0, top, width, bottom - top + 1);

        Rect roiInOriginal = new Rect(
            result.RoiInOriginal.X + roi.X,
            result.RoiInOriginal.Y + roi.Y,
            roi.Width,
            roi.Height
        );

        Mat croppedMat = new Mat(src, roi).Clone();

        return new CropResult(croppedMat, roiInOriginal);
    }
}
