namespace Fluxo.Domain.Rapprochement;

/// <summary>Un achat saisi pour lequel plusieurs lignes du relevé sont candidates
/// à égale distance, sans qu'on puisse trancher automatiquement.</summary>
public sealed class Ambiguite
{
    public required Guid AchatId { get; init; }
    public required IReadOnlyList<LigneReleve> Candidates { get; init; }
}
