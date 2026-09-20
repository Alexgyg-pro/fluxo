using System.Globalization;
using AlxBudgetDesktop.App.Mvvm;
using AlxBudgetDesktop.Data.Repositories;
using AlxBudgetDesktop.Domain.Models;

namespace AlxBudgetDesktop.App.ViewModels.Dialogs;

/// <summary>
/// Modale de calage manuel du solde d'ouverture d'un mois (les 3 comptes ensemble).
/// Pour chaque compte : rappel du report automatique en lecture seule, saisie du solde
/// réel, correction calculée en direct (saisie − report). La valeur persistée est le
/// solde réel saisi (ancre absolue), pas la correction elle-même.
/// </summary>
public sealed class CalageModalViewModel : ModalViewModelBase
{
    private readonly int _annee;
    private readonly string _cleMois;
    private readonly SoldesRepository _soldesRepository;

    public bool EstDejaCale { get; }

    public decimal ReportCourant { get; }
    public decimal ReportTr { get; }
    public decimal ReportEpargne { get; }

    private string _courantTexte;
    public string CourantTexte
    {
        get => _courantTexte;
        set { if (SetProperty(ref _courantTexte, value)) RecalculerCorrections(); }
    }

    private string _trTexte;
    public string TrTexte
    {
        get => _trTexte;
        set { if (SetProperty(ref _trTexte, value)) RecalculerCorrections(); }
    }

    private string _epargneTexte;
    public string EpargneTexte
    {
        get => _epargneTexte;
        set { if (SetProperty(ref _epargneTexte, value)) RecalculerCorrections(); }
    }

    public decimal CorrectionCourant => ParseDecimal(CourantTexte) - ReportCourant;
    public decimal CorrectionTr => ParseDecimal(TrTexte) - ReportTr;
    public decimal CorrectionEpargne => ParseDecimal(EpargneTexte) - ReportEpargne;

    public bool CorrectionCourantPositive => CorrectionCourant >= 0;
    public bool CorrectionTrPositive => CorrectionTr >= 0;
    public bool CorrectionEpargnePositive => CorrectionEpargne >= 0;

    public RelayCommand EnregistrerCommand { get; }
    public RelayCommand RevenirReportAutoCommand { get; }
    public RelayCommand AnnulerCommand { get; }

    public CalageModalViewModel(
        int annee,
        string cleMois,
        SoldesComptes report,
        SoldesComptes? calageExistant,
        SoldesRepository soldesRepository)
    {
        _annee = annee;
        _cleMois = cleMois;
        _soldesRepository = soldesRepository;

        ReportCourant = report.Courant;
        ReportTr = report.TicketRestaurant;
        ReportEpargne = report.Epargne;

        EstDejaCale = calageExistant is not null;
        _courantTexte = (calageExistant?.Courant ?? report.Courant).ToString("0.00", CultureInfo.InvariantCulture);
        _trTexte = (calageExistant?.TicketRestaurant ?? report.TicketRestaurant).ToString("0.00", CultureInfo.InvariantCulture);
        _epargneTexte = (calageExistant?.Epargne ?? report.Epargne).ToString("0.00", CultureInfo.InvariantCulture);

        EnregistrerCommand = new RelayCommand(Enregistrer);
        RevenirReportAutoCommand = new RelayCommand(RevenirReportAuto);
        AnnulerCommand = new RelayCommand(() => Close(false));
    }

    private void RecalculerCorrections()
    {
        OnPropertyChanged(nameof(CorrectionCourant));
        OnPropertyChanged(nameof(CorrectionTr));
        OnPropertyChanged(nameof(CorrectionEpargne));
        OnPropertyChanged(nameof(CorrectionCourantPositive));
        OnPropertyChanged(nameof(CorrectionTrPositive));
        OnPropertyChanged(nameof(CorrectionEpargnePositive));
    }

    private void Enregistrer()
    {
        var soldes = new SoldesComptes
        {
            Courant = ParseDecimal(CourantTexte),
            TicketRestaurant = ParseDecimal(TrTexte),
            Epargne = ParseDecimal(EpargneTexte),
        };

        _soldesRepository.DefinirCalage(_annee, _cleMois, soldes);
        Close(true);
    }

    private void RevenirReportAuto()
    {
        _soldesRepository.SupprimerCalage(_annee, _cleMois);
        Close(true);
    }

    private static decimal ParseDecimal(string texte)
        => decimal.TryParse(texte.Replace(',', '.'), NumberStyles.Number, CultureInfo.InvariantCulture, out var v) ? v : 0m;
}
