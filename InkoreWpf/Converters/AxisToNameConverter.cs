using System.Globalization;
using System.Windows.Data;
using InkoreWpf.Model;
using LeadshineCard.Core.Interfaces;
using LyuExtensions.Extensions;

namespace InkoreWpf.Converters;

public class AxisToNameConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is IAxisController axis)
        {
            return ((AxisEnum)axis.AxisNo).GetEnumDescription();
        }
        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
