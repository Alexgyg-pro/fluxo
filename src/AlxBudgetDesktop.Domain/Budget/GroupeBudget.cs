namespace AlxBudgetDesktop.Domain.Budget;

public sealed class GroupeBudget
{
    public required string Nom { get; init; }
    public required IReadOnlyList<LigneBudget> Lignes { get; init; }

    /// <summary>12 valeurs, index 0 = janvier.</summary>
    public required IReadOnlyList<decimal> TotalMensuel { get; init; }

    public required decimal Total { get; init; }
}
