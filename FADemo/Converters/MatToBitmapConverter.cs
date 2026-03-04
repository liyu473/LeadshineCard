using System;
using System.Globalization;
using Avalonia.Data.Converters;
using FADemo.Extensions;
using OpenCvSharp;

namespace FADemo.Converters;

public class MatToBitmapConverter:IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not Mat mat || mat.Empty())
            return null;

        return mat.ToAvaloniaBitmap();
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
