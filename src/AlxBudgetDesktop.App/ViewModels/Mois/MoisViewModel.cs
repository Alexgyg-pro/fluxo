using AlxBudgetDesktop.App.ViewModels.Shell;

namespace AlxBudgetDesktop.App.ViewModels.Mois;

/// <summary>
/// Squelette de l'onglet d'un mois. Le contenu réel (bandeau de soldes, liste des
/// opérations, contrôle de rapprochement...) sera développé dans une session dédiée
/// à l'écran de saisie manuelle — cette version n'affiche que le titre.
/// </summary>
public sealed class MoisViewModel : TabViewModelBase
{
    private static readonly string[] NomsMois =
    [
        "Janvier", "Février", "Mars", "Avril", "Mai", "Juin",
        "Juillet", "Août", "Septembre", "Octobre", "Novembre", "Décembre",
    ];

    /// <summary>Index 0 = janvier, 11 = décembre — cohérent avec le domaine métier.</summary>
    public int Mois { get; }

    public int Annee { get; }

    public MoisViewModel(int mois, int annee, bool estValide)
        : base(NomsMois[mois][..3], estValide)
    {
        Mois = mois;
        Annee = annee;
    }
}
