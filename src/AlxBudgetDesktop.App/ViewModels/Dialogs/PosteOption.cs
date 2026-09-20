namespace AlxBudgetDesktop.App.ViewModels.Dialogs;

/// <summary>Un poste sélectionnable dans la liste déroulante, avec son groupe
/// d'appartenance — sert uniquement à regrouper visuellement la liste (en-têtes de
/// groupe non cliquables), le groupe n'est pas persisté avec le choix de l'utilisateur.</summary>
public sealed class PosteOption
{
    public required string Poste { get; init; }
    public required string Groupe { get; init; }
}
