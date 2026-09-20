using AlxBudgetDesktop.Domain.Rapprochement;

namespace AlxBudgetDesktop.App.ViewModels.Mois;

/// <summary>Une ligne du relevé sans saisie correspondante ("à saisir") — affichée en
/// ligne fantôme cliquable à la fin de la liste des opérations du mois.</summary>
public sealed class LigneFantomeViewModel(LigneNonRapprochee source)
{
    public LigneNonRapprochee Source { get; } = source;

    public DateOnly DatePrelevement => Source.Ligne.DatePrelevement;
    public decimal Montant => Math.Abs(Source.Ligne.Montant);
    public string RaisonSociale => Source.Ligne.RaisonSociale;
}
