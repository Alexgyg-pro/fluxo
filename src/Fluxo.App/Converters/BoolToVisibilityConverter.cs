using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Fluxo.App.Converters;

/// <summary>true → Visible, false → Collapsed (pas Hidden : l'élément ne doit pas
/// réserver d'espace quand il est masqué, ex. la pastille ✓ sur un onglet non validé).</summary>
public sealed class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is true ? Visibility.Visible : Visibility.Collapsed;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
