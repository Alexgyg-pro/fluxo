using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Fluxo.App.Converters;

/// <summary>true → Collapsed, false → Visible (inverse de BoolToVisibilityConverter,
/// pour basculer entre la vue normale et la vue calée du bandeau de soldes).</summary>
public sealed class InverseBoolToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is true ? Visibility.Collapsed : Visibility.Visible;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
