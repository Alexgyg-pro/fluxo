namespace Fluxo.Domain.Models;

/// <summary>
/// Soldes calculés pour un mois donné (0 = janvier, 11 = décembre).
/// </summary>
public sealed class SoldesMois
{
    public required int Mois { get; init; }
    public required SoldesComptes Report { get; init; }
    public required SoldesComptes Ouverture { get; init; }
    public required SoldesComptes Mouvements { get; init; }
    public required SoldesComptes Disponible { get; init; }
    public required bool Cale { get; init; }
}
