using System;
using System.Globalization;
using System.Windows.Data;

namespace Rayman1Randomizer.Converters;

public class IntToStringConverter : IValueConverter
{
    public int DefaultValue { get; set; }

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value?.ToString() ?? String.Empty;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string str)
            return DefaultValue;

        return Int32.TryParse(str, out int v) ? v : DefaultValue;
    }
}