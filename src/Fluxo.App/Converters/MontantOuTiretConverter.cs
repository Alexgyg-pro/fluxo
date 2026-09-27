using System.Globalization;
using System.Windows.Data;

namespace Fluxo.App.Converters;

/// <summary>Affiche "—" pour un montant à zéro plutôt que "0,00 €" — reproduit la
/// fonction utilitaire eur() de l'onglet Budget côté alxBudget, qui distingue
/// visuellement "aucune donnée/valeur nulle" du reste des montants.</summary>
public sealed class MontantOuTiretConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is decimal d
            ? (d == 0 ? "—" : d.ToString("0.00", CultureInfo.InvariantCulture) + " €")
            : "";

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
