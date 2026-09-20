using AlxBudgetDesktop.Domain.Budget;

namespace AlxBudgetDesktop.App.ViewModels.Budget;

/// <summary>Une ligne du tableau pivot de l'onglet Budget : groupe, poste (indenté) ou
/// la ligne Solde finale. L'indexeur permet un binding WPF simple "[0]".."[11]" sur les
/// 12 colonnes mensuelles du DataGrid, sans dupliquer 12 propriétés à la main.</summary>
public sealed class LigneBudgetAffichage
{
    public required string Libelle { get; init; }
    public required IReadOnlyList<decimal> Valeurs { get; init; }
    public required decimal Total { get; init; }
    public bool EstGroupe { get; init; }
    public bool EstSolde { get; init; }

    public decimal this[int mois] => Valeurs[mois];

    public static LigneBudgetAffichage PourGroupe(GroupeBudget groupe) => new()
    {
        Libelle = groupe.Nom,
        Valeurs = groupe.TotalMensuel,
        Total = groupe.Total,
        EstGroupe = true,
    };

    public static LigneBudgetAffichage PourPoste(LigneBudget ligne) => new()
    {
        Libelle = "     " + ligne.Poste,
        Valeurs = ligne.Mensuel,
        Total = ligne.Total,
    };

    public static LigneBudgetAffichage PourSolde(IReadOnlyList<decimal> soldeMensuel, decimal soldeTotal) => new()
    {
        Libelle = "Solde",
        Valeurs = soldeMensuel,
        Total = soldeTotal,
        EstSolde = true,
    };
}
