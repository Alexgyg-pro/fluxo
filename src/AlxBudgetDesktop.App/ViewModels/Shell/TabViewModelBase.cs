using AlxBudgetDesktop.App.Mvvm;

namespace AlxBudgetDesktop.App.ViewModels.Shell;

/// <summary>Base commune à tous les onglets affichés dans la barre du shell principal.</summary>
public abstract class TabViewModelBase(string titre, bool estValide = false) : ObservableObject
{
    public string Titre { get; } = titre;

    private bool _estValide = estValide;

    /// <summary>Pastille ✓ dans la barre d'onglets — seuls les onglets mois l'utilisent,
    /// les autres restent à false. Porté sur la base pour un binding XAML uniforme,
    /// sans avoir à distinguer le type concret de l'onglet dans le template. Modifiable
    /// (pas seulement à la construction) pour refléter en direct une validation/
    /// dévalidation de mois.</summary>
    public bool EstValide
    {
        get => _estValide;
        protected set => SetProperty(ref _estValide, value);
    }
}
