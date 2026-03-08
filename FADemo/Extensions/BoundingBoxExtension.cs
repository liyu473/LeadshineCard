using LyuOnnxCore.Models;
using OpenCvSharp;

namespace FADemo.Extensions;

public static class BoundingBoxExtensions
{
    public static Rect ToRect(this BoundingBox box)
    {
        return new Rect(box.X, box.Y, box.Width, box.Height);
    }
}
