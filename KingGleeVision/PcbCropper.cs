using KingGleeVision.MatPreProcess;
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

    public static Mat? CropPcbArea(Mat src)
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

            return croppedExact;
        }

        return null;
    }
}
