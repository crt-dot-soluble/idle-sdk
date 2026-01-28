using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace IdleSdk.Demo.Converters;

public sealed class OneThirdWidthConverter : IValueConverter
{
    private const double MinWidth = 280;
    private const double MaxWidth = 520;

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double width)
        {
            var third = width / 3d;
            return Math.Clamp(third, MinWidth, MaxWidth);
        }

        return MinWidth;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
