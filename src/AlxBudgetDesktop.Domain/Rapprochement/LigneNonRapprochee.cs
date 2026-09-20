namespace AlxBudgetDesktop.Domain.Rapprochement;

/// <summary>Une ligne du relevé sans aucun achat saisi correspondant.</summary>
public sealed class LigneNonRapprochee
{
    public required LigneReleve Ligne { get; init; }
    public required IReadOnlyList<string> PostesSuggeres { get; init; }
}
