namespace Fluxo.Domain.Budget;

public sealed class LigneBudget
{
    public required string Poste { get; init; }

    /// <summary>12 valeurs, index 0 = janvier.</summary>
    public required IReadOnlyList<decimal> Mensuel { get; init; }

    public required decimal Total { get; init; }
}
