using OpenCvSharp;

namespace KingGleeVision.Extensions;

public static class RectExtension
{
    public static (int X, int Y) Center(this Rect rect)
    {
        int centerX = rect.X + rect.Width / 2;
        int centerY = rect.Y + rect.Height / 2;

        return (centerX, centerY);
    }
}
