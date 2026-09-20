using System.Collections.ObjectModel;
using AlxBudgetDesktop.App.Mvvm;
using AlxBudgetDesktop.App.ViewModels.Admin;
using AlxBudgetDesktop.App.ViewModels.Budget;
using AlxBudgetDesktop.App.ViewModels.Import;
using AlxBudgetDesktop.App.ViewModels.Mois;
using AlxBudgetDesktop.Data.Repositories;
using AlxBudgetDesktop.Domain.Services;

namespace AlxBudgetDesktop.App.ViewModels.Shell;

/// <summary>
/// ViewModel racine : construit les 12 onglets mois + Budget/Import/Admin pour l'année
/// courante et pilote l'onglet sélectionné. Pas de vrai routeur — le changement d'onglet
/// est une simple affectation de <see cref="OngletSelectionne"/>, rendue par un
/// DataTemplate keyé par type de ViewModel côté XAML (App.xaml). Les deux groupes
/// d'onglets sont exposés séparément pour permettre l'alignement gauche/droite de la
/// barre d'onglets côté vue (mois à gauche, Budget/Import/Admin à droite).
/// </summary>
public sealed class MainWindowViewModel : ObservableObject
{
    public int Annee { get; }

    public ObservableCollection<TabViewModelBase> OngletsMois { get; } = [];

    public ObservableCollection<TabViewModelBase> OngletsAutres { get; } = [];

    private TabViewModelBase _ongletSelectionne;
    public TabViewModelBase OngletSelectionne
    {
        get => _ongletSelectionne;
        set => SetProperty(ref _ongletSelectionne, value);
    }

    public MainWindowViewModel(
        ConfigRepository configRepository,
        ValidationsRepository validationsRepository,
        PostesRepository postesRepository,
        AchatsRepository achatsRepository,
        SoldesRepository soldesRepository,
        IDialogService dialogService)
    {
        Annee = configRepository.Charger().AnneeCourante;
        var validations = validationsRepository.Charger(Annee);

        for (var mois = 0; mois < 12; mois++)
        {
            var cleMois = SoldesService.CleMois(Annee, mois);
            OngletsMois.Add(new MoisViewModel(
                mois, Annee, estValide: validations.ContainsKey(cleMois),
                postesRepository, achatsRepository, soldesRepository, dialogService));
        }

        OngletsAutres.Add(new BudgetViewModel());
        OngletsAutres.Add(new ImportViewModel());
        OngletsAutres.Add(new AdminViewModel());

        // Sélectionne le mois courant au lancement (si l'année affichée est bien
        // l'année civile en cours) ; sinon janvier par défaut.
        var aujourdhui = DateTime.Now;
        _ongletSelectionne = aujourdhui.Year == Annee ? OngletsMois[aujourdhui.Month - 1] : OngletsMois[0];
    }
}
