using OpenCvSharp;

namespace KingGleeVision.MatPreProcess;

public static class DrawRect
{
    public static Mat DrawRectOnMat(Mat mat, Rect rect, Scalar? color = null, int thickness = 2)
    {
        var result = mat.Clone();

        Cv2.Rectangle(result, rect, color ?? new Scalar(0, 255, 0), thickness);

        return result;
    }

    public static Mat DrawRectEx(this Mat mat, Rect rect, Scalar? color = null, int thickness = 2)
    {
        var result = mat.Clone();

        Cv2.Rectangle(result, rect, color ?? new Scalar(0, 255, 0), thickness);

        return result;
    }
}
