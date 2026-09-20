using System.Collections.ObjectModel;
using Microsoft.Win32;
using AlxBudgetDesktop.App.Mvvm;
using AlxBudgetDesktop.App.ViewModels.Dialogs;
using AlxBudgetDesktop.App.ViewModels.Shell;
using AlxBudgetDesktop.Data.Csv;
using AlxBudgetDesktop.Data.Repositories;
using AlxBudgetDesktop.Domain.Csv;
using AlxBudgetDesktop.Domain.Models;
using AlxBudgetDesktop.Domain.Rapprochement;
using AlxBudgetDesktop.Domain.Services;

namespace AlxBudgetDesktop.App.ViewModels.Mois;

/// <summary>
/// Onglet d'un mois : bandeau de soldes (avec calage manuel), liste des opérations,
/// saisie manuelle (dépense/recette) et contrôle de rapprochement bancaire éphémère
/// (badges "orpheline"/"à saisir", ligne fantôme, validation/dévalidation de mois).
/// L'écran Import CSV complet (persisté, avec ses 3 blocs de confirmation) est un
/// écran séparé, développé dans une session dédiée ultérieure.
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
    private readonly ValidationsRepository _validationsRepository;
    private readonly IDialogService _dialogService;

    private List<GroupePostes> _groupes = [];

    /// <summary>Résultat du dernier contrôle de rapprochement lancé, null tant qu'aucun
    /// relevé n'a été chargé. Conservé (avec les lignes sources) pour pouvoir réanalyser
    /// automatiquement après tout ajout/modification/suppression d'opération.</summary>
    private ResultatRapprochement? _resultatControle;
    private List<LigneReleve>? _dernieresLignesReleve;

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

    // --- Contrôle de rapprochement bancaire (éphémère) ---

    private bool _aControle;
    public bool AControle { get => _aControle; private set => SetProperty(ref _aControle, value); }

    public string TexteBoutonControle => AControle ? "Recontrôler avec un relevé" : "Contrôler avec un relevé";

    private string _resumeControle = "";
    public string ResumeControle { get => _resumeControle; private set => SetProperty(ref _resumeControle, value); }

    private bool _controleSansEcart;
    public bool ControleSansEcart { get => _controleSansEcart; private set => SetProperty(ref _controleSansEcart, value); }

    private string? _messageErreurControle;
    public string? MessageErreurControle
    {
        get => _messageErreurControle;
        private set { if (SetProperty(ref _messageErreurControle, value)) OnPropertyChanged(nameof(AMessageErreurControle)); }
    }
    public bool AMessageErreurControle => !string.IsNullOrEmpty(MessageErreurControle);

    public ObservableCollection<LigneFantomeViewModel> LignesFantomes { get; } = [];

    // --- Validation du mois ---

    private DateOnly? _dateValidation;
    public DateOnly? DateValidation { get => _dateValidation; private set => SetProperty(ref _dateValidation, value); }

    public bool PeutValider => AControle && ControleSansEcart && !EstValide;

    public RelayCommand AjouterDepenseCommand { get; }
    public RelayCommand AjouterRecetteCommand { get; }
    public RelayCommand<AchatLigneViewModel> ModifierCommand { get; }
    public RelayCommand<AchatLigneViewModel> SupprimerCommand { get; }
    public RelayCommand OuvrirCalageCommand { get; }
    public RelayCommand ControlerCommand { get; }
    public RelayCommand<LigneFantomeViewModel> SaisirDepuisLigneFantomeCommand { get; }
    public RelayCommand ValiderCommand { get; }
    public RelayCommand DevaliderCommand { get; }

    public MoisViewModel(
        int mois,
        int annee,
        bool estValide,
        PostesRepository postesRepository,
        AchatsRepository achatsRepository,
        SoldesRepository soldesRepository,
        ValidationsRepository validationsRepository,
        IDialogService dialogService)
        : base(NomsMois[mois][..3], estValide)
    {
        Mois = mois;
        Annee = annee;
        _postesRepository = postesRepository;
        _achatsRepository = achatsRepository;
        _soldesRepository = soldesRepository;
        _validationsRepository = validationsRepository;
        _dialogService = dialogService;

        AjouterDepenseCommand = new RelayCommand(() => OuvrirModaleAjout(ModeAchat.Depense));
        AjouterRecetteCommand = new RelayCommand(() => OuvrirModaleAjout(ModeAchat.Recette));
        ModifierCommand = new RelayCommand<AchatLigneViewModel>(OuvrirModaleModification);
        SupprimerCommand = new RelayCommand<AchatLigneViewModel>(Supprimer);
        OuvrirCalageCommand = new RelayCommand(OuvrirCalage);
        ControlerCommand = new RelayCommand(Controler);
        SaisirDepuisLigneFantomeCommand = new RelayCommand<LigneFantomeViewModel>(SaisirDepuisLigneFantome);
        ValiderCommand = new RelayCommand(Valider, () => PeutValider);
        DevaliderCommand = new RelayCommand(Devalider, () => EstValide);

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

        var validations = _validationsRepository.Charger(Annee);
        var cleMois = SoldesService.CleMois(Annee, Mois);
        if (validations.TryGetValue(cleMois, out var dateTexte) && DateOnly.TryParse(dateTexte, out var date))
        {
            EstValide = true;
            DateValidation = date;
        }
        else
        {
            EstValide = false;
            DateValidation = null;
        }

        // Un contrôle actif est automatiquement réanalysé après tout changement des
        // opérations du mois (ajout/modification/suppression) — cf. NOTICE.md US-012.
        if (_dernieresLignesReleve is not null)
        {
            AnalyserControle(_dernieresLignesReleve, achatsAnnee, recettes);
        }

        NotifierEtatValidation();
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

    private void Controler()
    {
        var dialog = new OpenFileDialog
        {
            Title = "Sélectionner un relevé bancaire (CSV CIC)",
            Filter = "Fichiers CSV (*.csv)|*.csv|Tous les fichiers (*.*)|*.*",
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        try
        {
            var texte = CicCsvFileReader.LireTexte(dialog.FileName);
            var lignes = CicCsvParser.Parse(texte);
            MessageErreurControle = null;

            var achatsAnnee = _achatsRepository.ListerAchats(Annee);
            var recettes = SoldesService.PostesRecette(_groupes);
            AnalyserControle(lignes, achatsAnnee, recettes);
        }
        catch (Exception ex)
        {
            MessageErreurControle = $"Impossible d'analyser ce fichier : {ex.Message}";
        }
    }

    /// <summary>Ne porte que sur les dépenses du compte courant — les recettes suivent
    /// une autre logique de date, non couverte par ce contrôle (cf. NOTICE.md).</summary>
    private void AnalyserControle(IReadOnlyList<LigneReleve> lignes, IReadOnlyList<Achat> achatsAnnee, IReadOnlySet<string> recettes)
    {
        _dernieresLignesReleve = lignes.ToList();

        var achatsCourantDepenses = achatsAnnee
            .Where(a => a.Compte == Compte.Courant && !EstRecette(a, recettes))
            .ToList();

        _resultatControle = ReconciliationService.Rapprocher(achatsCourantDepenses, lignes);

        var orphelinesDuMois = _resultatControle.AchatsNonRapproches
            .Where(a => a.DateAchat.Year == Annee && a.DateAchat.Month - 1 == Mois)
            .Select(a => a.Id)
            .ToHashSet();

        for (var i = 0; i < Achats.Count; i++)
        {
            if (orphelinesDuMois.Contains(Achats[i].Id))
            {
                Achats[i] = new AchatLigneViewModel(Achats[i].Achat, Achats[i].EstRecette, estOrpheline: true);
            }
        }

        LignesFantomes.Clear();
        foreach (var ligne in _resultatControle.LignesNonRapprochees.Where(l =>
                     l.Ligne.Montant < 0 && l.Ligne.DatePrelevement.Year == Annee && l.Ligne.DatePrelevement.Month - 1 == Mois))
        {
            LignesFantomes.Add(new LigneFantomeViewModel(ligne));
        }

        var nbEcarts = orphelinesDuMois.Count + LignesFantomes.Count;
        ControleSansEcart = nbEcarts == 0;
        ResumeControle = ControleSansEcart
            ? "✓ Tout est rapproché pour ce mois"
            : $"Contrôle : {orphelinesDuMois.Count} orpheline(s), {LignesFantomes.Count} à saisir";

        AControle = true;
        OnPropertyChanged(nameof(TexteBoutonControle));
        NotifierEtatValidation();
    }

    private void SaisirDepuisLigneFantome(LigneFantomeViewModel? ligne)
    {
        if (ligne is null)
        {
            return;
        }

        var prefill = new AchatPrefill
        {
            Montant = ligne.Montant,
            Etiquette = ligne.RaisonSociale,
            DatePrelevement = ligne.DatePrelevement,
            RaisonSociale = ligne.RaisonSociale,
            LibelleReleve = ligne.Source.Ligne.LibelleBrut,
        };

        var vm = new AchatModalViewModel(ModeAchat.Depense, _groupes, _achatsRepository, DateParDefaut(), prefill: prefill);
        if (_dialogService.ShowAchatModal(vm))
        {
            Charger();
        }
    }

    private void Valider()
    {
        var cleMois = SoldesService.CleMois(Annee, Mois);
        var aujourdhui = DateOnly.FromDateTime(DateTime.Now);
        _validationsRepository.Valider(Annee, cleMois, aujourdhui);
        EstValide = true;
        DateValidation = aujourdhui;
        NotifierEtatValidation();
    }

    private void Devalider()
    {
        var cleMois = SoldesService.CleMois(Annee, Mois);
        _validationsRepository.Devalider(Annee, cleMois);
        EstValide = false;
        DateValidation = null;
        NotifierEtatValidation();
    }

    private void NotifierEtatValidation()
    {
        OnPropertyChanged(nameof(PeutValider));
        ValiderCommand.RaiseCanExecuteChanged();
        DevaliderCommand.RaiseCanExecuteChanged();
    }
}
