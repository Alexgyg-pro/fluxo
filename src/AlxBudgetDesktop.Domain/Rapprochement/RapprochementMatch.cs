namespace AlxBudgetDesktop.Domain.Rapprochement;

/// <summary>Un appariement automatique entre un achat saisi et une ligne du relevé.</summary>
public sealed class RapprochementMatch
{
    public required Guid AchatId { get; init; }
    public required LigneReleve Ligne { get; init; }
}
