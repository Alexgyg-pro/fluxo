using AlxBudgetDesktop.App.Mvvm;

namespace AlxBudgetDesktop.App.ViewModels.Shell;

/// <summary>Base commune à tous les onglets affichés dans la barre du shell principal.</summary>
public abstract class TabViewModelBase(string titre, bool estValide = false) : ObservableObject
{
    public string Titre { get; } = titre;

    /// <summary>Pastille ✓ dans la barre d'onglets — seuls les onglets mois l'utilisent,
    /// les autres restent à false. Porté sur la base pour un binding XAML uniforme,
    /// sans avoir à distinguer le type concret de l'onglet dans le template.</summary>
    public bool EstValide { get; } = estValide;
}
