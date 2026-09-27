using System.Collections.ObjectModel;
using System.Linq;
using Fluxo.App.Mvvm;
using Fluxo.App.ViewModels.Admin;
using Fluxo.App.ViewModels.Aide;
using Fluxo.App.ViewModels.Budget;
using Fluxo.App.ViewModels.Mois;
using Fluxo.Data.Repositories;
using Fluxo.Domain.Services;

namespace Fluxo.App.ViewModels.Shell;

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
    private readonly PreferencesRepository _preferencesRepository;
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
            var ancien = _ongletSelectionne;
            if (!SetProperty(ref _ongletSelectionne, value))
            {
                return;
            }

            // EstSelectionne piloté ici, explicitement, sur les deux onglets concernés —
            // c'est ce qui remplace le binding SelectedItem de ListBox (cf. TabViewModelBase).
            if (ancien is not null)
            {
                ancien.EstSelectionne = false;
            }
            value.EstSelectionne = true;

            // Rafraîchit le contenu à chaque activation de l'onglet, pour refléter les
            // changements faits depuis un autre onglet (saisies, édition des postes en
            // Admin...) sans avoir à relancer l'application.
            switch (value)
            {
                case BudgetViewModel budget:
                    budget.Rafraichir();
                    break;
                case MoisViewModel mois:
                    mois.Rafraichir();
                    break;
            }
        }
    }

    public RelayCommand<TabViewModelBase> SelectionnerOngletCommand { get; }

    public RelayCommand OuvrirAideCommand { get; }

    public MainWindowViewModel(
        ConfigRepository configRepository,
        ValidationsRepository validationsRepository,
        PostesRepository postesRepository,
        AchatsRepository achatsRepository,
        SoldesRepository soldesRepository,
        PreferencesRepository preferencesRepository,
        IDialogService dialogService)
    {
        _postesRepository = postesRepository;
        _achatsRepository = achatsRepository;
        _soldesRepository = soldesRepository;
        _validationsRepository = validationsRepository;
        _preferencesRepository = preferencesRepository;
        _dialogService = dialogService;

        _annee = configRepository.Charger().AnneeCourante;

        ConstruireOngletsMois();

        var adminViewModel = new AdminViewModel(Annee, configRepository, postesRepository, dialogService);
        adminViewModel.AnneeEnregistree += ChangerAnnee;

        OngletsAutres.Add(new BudgetViewModel(Annee, postesRepository, achatsRepository));
        // Import volontairement masqué (BACKLOG US-003) : en pause faute d'export CSV
        // bancaire testable, et le contrôle de rapprochement du mois couvre déjà une
        // bonne partie du besoin. ImportViewModel reste en place, prêt à être réactivé.
        OngletsAutres.Add(adminViewModel);

        SelectionnerOngletCommand = new RelayCommand<TabViewModelBase>(onglet =>
        {
            if (onglet is not null)
            {
                OngletSelectionne = onglet;
            }
        });

        OuvrirAideCommand = new RelayCommand(OuvrirAide);

        _ongletSelectionne = ChoisirOngletParDefaut();
        _ongletSelectionne.EstSelectionne = true;
    }

    /// <summary>Ouvre l'aide en ligne (fenêtre à thèmes, contenu Markdown embarqué) —
    /// non modale, cf. IDialogService.ShowAide.</summary>
    private void OuvrirAide() => _dialogService.ShowAide(new AideViewModel());

    private void ConstruireOngletsMois()
    {
        OngletsMois.Clear();
        var validations = _validationsRepository.Charger(Annee);

        for (var mois = 0; mois < 12; mois++)
        {
            var cleMois = SoldesService.CleMois(Annee, mois);
            OngletsMois.Add(new MoisViewModel(
                mois, Annee, estValide: validations.ContainsKey(cleMois),
                _postesRepository, _achatsRepository, _soldesRepository, _validationsRepository,
                _preferencesRepository, _dialogService));
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
