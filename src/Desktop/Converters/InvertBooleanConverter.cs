using System.Globalization;
using System.Windows.Data;

namespace Desktop.Converters;

public class InvertBooleanConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is bool booleanValue
            ? !booleanValue
            : value;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is bool booleanValue
            ? !booleanValue
            : value;
    }
}