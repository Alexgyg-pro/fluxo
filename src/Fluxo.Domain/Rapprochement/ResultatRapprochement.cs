using Fluxo.Domain.Models;

namespace Fluxo.Domain.Rapprochement;

public sealed class ResultatRapprochement
{
    public required IReadOnlyList<RapprochementMatch> Matches { get; init; }
    public required IReadOnlyList<Ambiguite> Ambigus { get; init; }
    public required IReadOnlyList<LigneNonRapprochee> LignesNonRapprochees { get; init; }
    public required IReadOnlyList<Achat> AchatsNonRapproches { get; init; }
}
