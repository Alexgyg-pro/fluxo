using System.Globalization;
using System.Windows.Data;

namespace AlxBudgetDesktop.App.Converters;

/// <summary>Pont entre le DateOnly du domaine et le DateTime? attendu par le DatePicker WPF natif.</summary>
public sealed class DateOnlyToDateTimeConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is DateOnly d ? d.ToDateTime(TimeOnly.MinValue) : null;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is DateTime dt ? DateOnly.FromDateTime(dt) : Binding.DoNothing;
}
