namespace Fluxo.Domain.Models;

public sealed class SoldesAnnee
{
    public required int Annee { get; init; }

    /// <summary>12 entrées, index 0 = janvier.</summary>
    public required IReadOnlyList<SoldesMois> Mois { get; init; }
}
