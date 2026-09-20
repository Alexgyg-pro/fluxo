using System.Collections.ObjectModel;
using AlxBudgetDesktop.App.Mvvm;
using AlxBudgetDesktop.App.ViewModels.Dialogs;
using AlxBudgetDesktop.App.ViewModels.Shell;
using AlxBudgetDesktop.Data.Repositories;
using AlxBudgetDesktop.Domain.Models;
using AlxBudgetDesktop.Domain.Services;

namespace AlxBudgetDesktop.App.ViewModels.Mois;

/// <summary>
/// Onglet d'un mois : bandeau de soldes (avec calage manuel), liste des opérations et
/// saisie manuelle (dépense/recette). Le contrôle de rapprochement bancaire et la
/// validation de mois sont hors scope ici, développés dans une session dédiée ultérieure.
/// </summary>
public sealed class MoisViewModel : TabViewModelBase
{
    private static readonly string[] NomsMois =
    [
        "Janvier", "Février", "Mars", "Avril", "Mai", "Juin",
        "Juillet", "Août", "Septembre", "Octobre", "Novembre", "Décembre",
    ];

    private readonly PostesRepository _postesRepository;
    private readonly AchatsRepository _achatsRepository;
    private readonly SoldesRepository _soldesRepository;
    private readonly IDialogService _dialogService;

    private List<GroupePostes> _groupes = [];

    /// <summary>Index 0 = janvier, 11 = décembre — cohérent avec le domaine métier.</summary>
    public int Mois { get; }

    public int Annee { get; }

    public string NomMois => NomsMois[Mois];

    public ObservableCollection<AchatLigneViewModel> Achats { get; } = [];

    private decimal _totalDepenses;
    public decimal TotalDepenses { get => _totalDepenses; private set => SetProperty(ref _totalDepenses, value); }

    private decimal _totalRecettes;
    public decimal TotalRecettes { get => _totalRecettes; private set => SetProperty(ref _totalRecettes, value); }

    private decimal _soldeMois;
    public decimal SoldeMois
    {
        get => _soldeMois;
        private set
        {
            if (SetProperty(ref _soldeMois, value))
            {
                OnPropertyChanged(nameof(SoldeMoisPositif));
            }
        }
    }

    public bool SoldeMoisPositif => SoldeMois >= 0;

    private decimal _ouvertureCourant, _mouvementsCourant, _disponibleCourant;
    public decimal OuvertureCourant { get => _ouvertureCourant; private set => SetProperty(ref _ouvertureCourant, value); }
    public decimal MouvementsCourant { get => _mouvementsCourant; private set => SetProperty(ref _mouvementsCourant, value); }
    public decimal DisponibleCourant { get => _disponibleCourant; private set => SetProperty(ref _disponibleCourant, value); }

    private decimal _ouvertureTr, _mouvementsTr, _disponibleTr;
    public decimal OuvertureTr { get => _ouvertureTr; private set => SetProperty(ref _ouvertureTr, value); }
    public decimal MouvementsTr { get => _mouvementsTr; private set => SetProperty(ref _mouvementsTr, value); }
    public decimal DisponibleTr { get => _disponibleTr; private set => SetProperty(ref _disponibleTr, value); }

    private decimal _ouvertureEpargne, _mouvementsEpargne, _disponibleEpargne;
    public decimal OuvertureEpargne { get => _ouvertureEpargne; private set => SetProperty(ref _ouvertureEpargne, value); }
    public decimal MouvementsEpargne { get => _mouvementsEpargne; private set => SetProperty(ref _mouvementsEpargne, value); }
    public decimal DisponibleEpargne { get => _disponibleEpargne; private set => SetProperty(ref _disponibleEpargne, value); }

    private bool _estCale;
    public bool EstCale { get => _estCale; private set => SetProperty(ref _estCale, value); }

    private decimal _reportCourant, _reportTr, _reportEpargne;
    public decimal ReportCourant { get => _reportCourant; private set => SetProperty(ref _reportCourant, value); }
    public decimal ReportTr { get => _reportTr; private set => SetProperty(ref _reportTr, value); }
    public decimal ReportEpargne { get => _reportEpargne; private set => SetProperty(ref _reportEpargne, value); }

    private decimal _correctionCourant, _correctionTr, _correctionEpargne;
    public decimal CorrectionCourant
    {
        get => _correctionCourant;
        private set { if (SetProperty(ref _correctionCourant, value)) OnPropertyChanged(nameof(CorrectionCourantPositive)); }
    }
    public decimal CorrectionTr
    {
        get => _correctionTr;
        private set { if (SetProperty(ref _correctionTr, value)) OnPropertyChanged(nameof(CorrectionTrPositive)); }
    }
    public decimal CorrectionEpargne
    {
        get => _correctionEpargne;
        private set { if (SetProperty(ref _correctionEpargne, value)) OnPropertyChanged(nameof(CorrectionEpargnePositive)); }
    }

    public bool CorrectionCourantPositive => CorrectionCourant >= 0;
    public bool CorrectionTrPositive => CorrectionTr >= 0;
    public bool CorrectionEpargnePositive => CorrectionEpargne >= 0;

    public RelayCommand AjouterDepenseCommand { get; }
    public RelayCommand AjouterRecetteCommand { get; }
    public RelayCommand<AchatLigneViewModel> ModifierCommand { get; }
    public RelayCommand<AchatLigneViewModel> SupprimerCommand { get; }
    public RelayCommand OuvrirCalageCommand { get; }

    public MoisViewModel(
        int mois,
        int annee,
        bool estValide,
        PostesRepository postesRepository,
        AchatsRepository achatsRepository,
        SoldesRepository soldesRepository,
        IDialogService dialogService)
        : base(NomsMois[mois][..3], estValide)
    {
        Mois = mois;
        Annee = annee;
        _postesRepository = postesRepository;
        _achatsRepository = achatsRepository;
        _soldesRepository = soldesRepository;
        _dialogService = dialogService;

        AjouterDepenseCommand = new RelayCommand(() => OuvrirModaleAjout(ModeAchat.Depense));
        AjouterRecetteCommand = new RelayCommand(() => OuvrirModaleAjout(ModeAchat.Recette));
        ModifierCommand = new RelayCommand<AchatLigneViewModel>(OuvrirModaleModification);
        SupprimerCommand = new RelayCommand<AchatLigneViewModel>(Supprimer);
        OuvrirCalageCommand = new RelayCommand(OuvrirCalage);

        Charger();
    }

    private void Charger()
    {
        _groupes = _postesRepository.Charger();

        var achatsAnnee = _achatsRepository.ListerAchats(Annee);
        var recettes = SoldesService.PostesRecette(_groupes);

        var achatsDuMois = achatsAnnee
            .Where(a => a.DateAchat.Month - 1 == Mois)
            .OrderBy(a => a.DateAchat)
            .ToList();

        Achats.Clear();
        foreach (var achat in achatsDuMois)
        {
            Achats.Add(new AchatLigneViewModel(achat, EstRecette(achat, recettes)));
        }

        TotalDepenses = Achats.Where(a => !a.EstRecette).Sum(a => a.MontantAchat);
        TotalRecettes = Achats.Where(a => a.EstRecette).Sum(a => a.MontantAchat);
        SoldeMois = TotalRecettes - TotalDepenses;

        var calages = _soldesRepository.ChargerCalages(Annee);
        var soldesAnnee = SoldesService.CalculerSoldesAnnee(Annee, achatsAnnee, calages, _groupes);
        var soldesMois = soldesAnnee.Mois[Mois];

        OuvertureCourant = soldesMois.Ouverture.Courant;
        MouvementsCourant = soldesMois.Mouvements.Courant;
        DisponibleCourant = soldesMois.Disponible.Courant;

        OuvertureTr = soldesMois.Ouverture.TicketRestaurant;
        MouvementsTr = soldesMois.Mouvements.TicketRestaurant;
        DisponibleTr = soldesMois.Disponible.TicketRestaurant;

        OuvertureEpargne = soldesMois.Ouverture.Epargne;
        MouvementsEpargne = soldesMois.Mouvements.Epargne;
        DisponibleEpargne = soldesMois.Disponible.Epargne;

        EstCale = soldesMois.Cale;
        ReportCourant = soldesMois.Report.Courant;
        ReportTr = soldesMois.Report.TicketRestaurant;
        ReportEpargne = soldesMois.Report.Epargne;
        CorrectionCourant = OuvertureCourant - ReportCourant;
        CorrectionTr = OuvertureTr - ReportTr;
        CorrectionEpargne = OuvertureEpargne - ReportEpargne;
    }

    private static bool EstRecette(Achat achat, IReadOnlySet<string> recettes) => achat.Postes.Keys.Any(recettes.Contains);

    /// <summary>US-009 : date par défaut à la création — aujourd'hui si l'onglet affiché
    /// est le mois courant, sinon le 1er jour de ce mois.</summary>
    private DateOnly DateParDefaut()
    {
        var aujourdhui = DateOnly.FromDateTime(DateTime.Now);
        return aujourdhui.Year == Annee && aujourdhui.Month - 1 == Mois ? aujourdhui : new DateOnly(Annee, Mois + 1, 1);
    }

    private void OuvrirModaleAjout(ModeAchat mode)
    {
        var vm = new AchatModalViewModel(mode, _groupes, _achatsRepository, DateParDefaut());
        if (_dialogService.ShowAchatModal(vm))
        {
            Charger();
        }
    }

    private void OuvrirModaleModification(AchatLigneViewModel? ligne)
    {
        if (ligne is null)
        {
            return;
        }

        var mode = ligne.EstRecette ? ModeAchat.Recette : ModeAchat.Depense;
        var vm = new AchatModalViewModel(mode, _groupes, _achatsRepository, DateParDefaut(), ligne.Achat);
        if (_dialogService.ShowAchatModal(vm))
        {
            Charger();
        }
    }

    private void OuvrirCalage()
    {
        var report = new SoldesComptes { Courant = ReportCourant, TicketRestaurant = ReportTr, Epargne = ReportEpargne };
        var calageExistant = EstCale
            ? new SoldesComptes { Courant = OuvertureCourant, TicketRestaurant = OuvertureTr, Epargne = OuvertureEpargne }
            : null;

        var vm = new CalageModalViewModel(Annee, SoldesService.CleMois(Annee, Mois), report, calageExistant, _soldesRepository);
        if (_dialogService.ShowCalageModal(vm))
        {
            Charger();
        }
    }

    private void Supprimer(AchatLigneViewModel? ligne)
    {
        if (ligne is null)
        {
            return;
        }

        var vm = new ConfirmDeleteModalViewModel(ligne.EstRecette, ligne.Etiquette, ligne.MontantAchat);
        if (_dialogService.ShowConfirmDelete(vm))
        {
            _achatsRepository.Supprimer(Annee, ligne.Id);
            Charger();
        }
    }
}
