namespace Fluxo.Domain.Budget;

public sealed class BudgetResult
{
    public required int Annee { get; init; }

    /// <summary>12 valeurs, index 0 = janvier.</summary>
    public required IReadOnlyList<TypeMois> TypesMois { get; init; }

    public required IReadOnlyList<GroupeBudget> Groupes { get; init; }

    /// <summary>12 valeurs : Revenus − tous les autres groupes (Remises comprises).</summary>
    public required IReadOnlyList<decimal> SoldeMensuel { get; init; }

    public required decimal SoldeTotal { get; init; }

    /// <summary>Cumul sur tout l'historique (toutes années), pas seulement l'année affichée.</summary>
    public required decimal SoldeEpargne { get; init; }

    /// <summary>Cumul sur tout l'historique (toutes années), pas seulement l'année affichée.</summary>
    public required decimal SoldeTicketsRestaurant { get; init; }
}
