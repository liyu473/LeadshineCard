using System;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using OpenCvSharp;

namespace FADemo.Extensions;

public static class MatExtension
{
    public static Bitmap ToAvaloniaBitmap(this Mat mat)
    {
        if (mat.Empty())
            throw new ArgumentException("Mat is empty");

        // 如果是灰度图，转成 BGRA
        if (mat.Type() == MatType.CV_8UC1)
        {
            Cv2.CvtColor(mat, mat, ColorConversionCodes.GRAY2BGRA);
        }
        else if (mat.Type() == MatType.CV_8UC3)
        {
            Cv2.CvtColor(mat, mat, ColorConversionCodes.BGR2BGRA);
        }

        return new Bitmap(
            PixelFormat.Bgra8888,
            AlphaFormat.Unpremul,
            mat.Data,
            new Avalonia.PixelSize(mat.Width, mat.Height),
            new Avalonia.Vector(96, 96),
            (int)mat.Step()
        );
    }
}
