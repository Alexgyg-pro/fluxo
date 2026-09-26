using System.IO;
using System.Windows;
using System.Windows.Documents;
using AlxBudgetDesktop.App.Aide;
using AlxBudgetDesktop.App.Mvvm;

namespace AlxBudgetDesktop.App.ViewModels.Aide;

/// <summary>
/// Aide en ligne : une liste de thèmes à gauche, le contenu Markdown (converti en
/// FlowDocument) du thème sélectionné à droite. Contenu embarqué en ressources WPF
/// (Aide/*.md dans AlxBudgetDesktop.App.csproj), pas de fichiers à côté de l'exe.
/// </summary>
public sealed class AideViewModel : ObservableObject
{
    public IReadOnlyList<SujetAideViewModel> Sujets { get; } =
    [
        new("Accueil", "Accueil.md"),
        new("Saisir une opération", "SaisirUneOperation.md"),
        new("Soldes du mois", "SoldesDuMois.md"),
        new("Contrôle bancaire", "ControleBancaire.md"),
        new("Valider un mois", "ValiderUnMois.md"),
        new("Gérer vos postes", "GererVosPostes.md"),
        new("Onglet Budget", "Budget.md"),
        new("Vos données", "VosDonnees.md"),
    ];

    private SujetAideViewModel _sujetSelectionne;
    public SujetAideViewModel SujetSelectionne
    {
        get => _sujetSelectionne;
        private set
        {
            var ancien = _sujetSelectionne;
            if (!SetProperty(ref _sujetSelectionne, value))
            {
                return;
            }

            if (ancien is not null)
            {
                ancien.EstSelectionne = false;
            }
            value.EstSelectionne = true;
            Contenu = ChargerContenu(value.NomFichier);
        }
    }

    private FlowDocument _contenu;
    public FlowDocument Contenu { get => _contenu; private set => SetProperty(ref _contenu, value); }

    public RelayCommand<SujetAideViewModel> SelectionnerSujetCommand { get; }

    public AideViewModel()
    {
        SelectionnerSujetCommand = new RelayCommand<SujetAideViewModel>(sujet =>
        {
            if (sujet is not null)
            {
                SujetSelectionne = sujet;
            }
        });

        _sujetSelectionne = Sujets[0];
        _sujetSelectionne.EstSelectionne = true;
        _contenu = ChargerContenu(_sujetSelectionne.NomFichier);
    }

    private static FlowDocument ChargerContenu(string nomFichier)
    {
        var uri = new Uri($"pack://application:,,,/Aide/{nomFichier}", UriKind.Absolute);
        var ressource = Application.GetResourceStream(uri)
            ?? throw new InvalidOperationException($"Ressource d'aide introuvable : {nomFichier}");

        using var lecteur = new StreamReader(ressource.Stream);
        return MarkdownVersFlowDocument.Convertir(lecteur.ReadToEnd());
    }
}
