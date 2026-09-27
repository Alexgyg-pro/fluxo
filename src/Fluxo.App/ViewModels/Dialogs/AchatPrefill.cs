namespace Fluxo.App.ViewModels.Dialogs;

/// <summary>Valeurs tirées d'une ligne de relevé "à saisir" pour pré-remplir la modale
/// de saisie quand l'utilisateur clique sur la ligne fantôme correspondante.</summary>
public sealed class AchatPrefill
{
    public required decimal Montant { get; init; }
    public required string Etiquette { get; init; }
    public required DateOnly DatePrelevement { get; init; }
    public string RaisonSociale { get; init; } = "";
    public string? LibelleReleve { get; init; }
}
