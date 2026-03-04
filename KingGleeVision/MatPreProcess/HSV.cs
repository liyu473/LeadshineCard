using OpenCvSharp;

namespace KingGleeVision.MatPreProcess;

public static class HSV
{
    public static Mat GetHSV(Mat src)
    {
        var hsv = new Mat();
        Cv2.CvtColor(src, hsv, ColorConversionCodes.BGR2HSV);
        return hsv;
    }

    public static Mat ToHSV(this Mat src)
    {
        var hsv = new Mat();
        Cv2.CvtColor(src, hsv, ColorConversionCodes.BGR2HSV);
        return hsv;
    }

    public static Mat GetSChannelWithOtsuFromHsv(Mat hsv)
    {
        Mat binary = new Mat();
        var channels = Cv2.Split(hsv);
        binary = channels[1];
        Cv2.Threshold(binary, binary, 0, 255, ThresholdTypes.Otsu);

        return binary;
    }

    public static Mat ToSChannelWithOtsuFromHsv(this Mat hsv)
    {
        Mat binary = new Mat();
        var channels = Cv2.Split(hsv);
        binary = channels[1];
        Cv2.Threshold(binary, binary, 0, 255, ThresholdTypes.Otsu);

        return binary;
    }
}
