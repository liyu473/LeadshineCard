using KingGleeVision.MatPreProcess;
using OpenCvSharp;

namespace KingGleeVision;

public static class PcbCropper
{
    public static Mat? CropPcbArea(Mat src)
    {
        return src.ToGaussianBlur(new Size(3, 3))
            .ToHSV()
            .ToSChannelWithOtsuFromHsv()
            .ToMedianBlur(3);
    }
}
