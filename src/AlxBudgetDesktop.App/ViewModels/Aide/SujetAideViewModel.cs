using AlxBudgetDesktop.App.Mvvm;

namespace AlxBudgetDesktop.App.ViewModels.Aide;

/// <summary>Un thème de l'aide en ligne : un titre affiché dans la colonne de gauche,
/// et le nom du fichier Markdown (ressource embarquée) à charger quand il est choisi.</summary>
public sealed class SujetAideViewModel(string titre, string nomFichier) : ObservableObject
{
    public string Titre { get; } = titre;
    public string NomFichier { get; } = nomFichier;

    private bool _estSelectionne;
    public bool EstSelectionne { get => _estSelectionne; set => SetProperty(ref _estSelectionne, value); }
}
