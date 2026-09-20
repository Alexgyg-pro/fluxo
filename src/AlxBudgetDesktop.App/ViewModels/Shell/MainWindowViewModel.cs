using System.Collections.ObjectModel;
using System.Linq;
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
/// Un changement d'année depuis l'onglet Admin reconstruit entièrement les onglets mois
/// et l'onglet Budget (postes.json reste partagé, seuls les achats/soldes/validations
/// sont partitionnés par année).
/// </summary>
public sealed class MainWindowViewModel : ObservableObject
{
    private readonly PostesRepository _postesRepository;
    private readonly AchatsRepository _achatsRepository;
    private readonly SoldesRepository _soldesRepository;
    private readonly ValidationsRepository _validationsRepository;
    private readonly IDialogService _dialogService;

    private int _annee;
    public int Annee { get => _annee; private set => SetProperty(ref _annee, value); }

    public ObservableCollection<TabViewModelBase> OngletsMois { get; } = [];

    public ObservableCollection<TabViewModelBase> OngletsAutres { get; } = [];

    private TabViewModelBase _ongletSelectionne;
    public TabViewModelBase OngletSelectionne
    {
        get => _ongletSelectionne;
        set
        {
            if (SetProperty(ref _ongletSelectionne, value) && value is BudgetViewModel budget)
            {
                // Rafraîchit le tableau à chaque activation de l'onglet, pour refléter
                // les saisies faites depuis les onglets mois sans relancer l'appli.
                budget.Rafraichir();
            }
        }
    }

    public MainWindowViewModel(
        ConfigRepository configRepository,
        ValidationsRepository validationsRepository,
        PostesRepository postesRepository,
        AchatsRepository achatsRepository,
        SoldesRepository soldesRepository,
        IDialogService dialogService)
    {
        _postesRepository = postesRepository;
        _achatsRepository = achatsRepository;
        _soldesRepository = soldesRepository;
        _validationsRepository = validationsRepository;
        _dialogService = dialogService;

        _annee = configRepository.Charger().AnneeCourante;

        ConstruireOngletsMois();

        var adminViewModel = new AdminViewModel(Annee, configRepository, postesRepository, dialogService);
        adminViewModel.AnneeEnregistree += ChangerAnnee;

        OngletsAutres.Add(new BudgetViewModel(Annee, postesRepository, achatsRepository));
        OngletsAutres.Add(new ImportViewModel());
        OngletsAutres.Add(adminViewModel);

        _ongletSelectionne = ChoisirOngletParDefaut();
    }

    private void ConstruireOngletsMois()
    {
        OngletsMois.Clear();
        var validations = _validationsRepository.Charger(Annee);

        for (var mois = 0; mois < 12; mois++)
        {
            var cleMois = SoldesService.CleMois(Annee, mois);
            OngletsMois.Add(new MoisViewModel(
                mois, Annee, estValide: validations.ContainsKey(cleMois),
                _postesRepository, _achatsRepository, _soldesRepository, _validationsRepository, _dialogService));
        }
    }

    /// <summary>Le mois courant si l'année affichée est bien l'année civile en cours,
    /// sinon janvier — utilisé au lancement et après un changement d'année.</summary>
    private TabViewModelBase ChoisirOngletParDefaut()
    {
        var aujourdhui = DateTime.Now;
        return aujourdhui.Year == Annee ? OngletsMois[aujourdhui.Month - 1] : OngletsMois[0];
    }

    private void ChangerAnnee(int nouvelleAnnee)
    {
        Annee = nouvelleAnnee;
        ConstruireOngletsMois();

        var indexBudget = OngletsAutres.ToList().FindIndex(o => o is BudgetViewModel);
        if (indexBudget >= 0)
        {
            OngletsAutres[indexBudget] = new BudgetViewModel(Annee, _postesRepository, _achatsRepository);
        }

        OngletSelectionne = ChoisirOngletParDefaut();
    }
}
