using System.Collections.ObjectModel;
using Fluxo.App.Mvvm;
using Fluxo.App.ViewModels.Dialogs;
using Fluxo.App.ViewModels.Shell;
using Fluxo.Data.Repositories;
using Fluxo.Domain.Models;

namespace Fluxo.App.ViewModels.Admin;

/// <summary>
/// Onglet Admin : changer l'année gérée par l'application, et éditer les groupes/postes
/// (ajout/renommage/suppression) en brouillon local avant sauvegarde explicite. Port
/// d'AdminTab.tsx — mêmes règles : rien n'est persisté avant "Enregistrer les postes"
/// (nettoyage des noms vides à ce moment-là), "Enregistrer l'année" n'affiche pas la
/// modale de confirmation (seul "Enregistrer les postes" le fait).
/// </summary>
public sealed class AdminViewModel : TabViewModelBase
{
    private readonly ConfigRepository _configRepository;
    private readonly PostesRepository _postesRepository;
    private readonly IDialogService _dialogService;

    /// <summary>Levé après un enregistrement réussi de l'année, pour que le shell
    /// reconstruise les onglets mois/Budget sur la nouvelle année.</summary>
    public event Action<int>? AnneeEnregistree;

    private string _anneeTexte;
    public string AnneeTexte { get => _anneeTexte; set => SetProperty(ref _anneeTexte, value); }

    private string? _erreurAnnee;
    public string? ErreurAnnee { get => _erreurAnnee; private set { if (SetProperty(ref _erreurAnnee, value)) OnPropertyChanged(nameof(AErreurAnnee)); } }
    public bool AErreurAnnee => !string.IsNullOrEmpty(ErreurAnnee);

    public ObservableCollection<GroupeEditionViewModel> Groupes { get; } = [];

    public RelayCommand EnregistrerAnneeCommand { get; }
    public RelayCommand AjouterGroupeCommand { get; }
    public RelayCommand<GroupeEditionViewModel> SupprimerGroupeCommand { get; }
    public RelayCommand EnregistrerPostesCommand { get; }

    public AdminViewModel(int anneeActuelle, ConfigRepository configRepository, PostesRepository postesRepository, IDialogService dialogService)
        : base("⚙️ Admin")
    {
        _configRepository = configRepository;
        _postesRepository = postesRepository;
        _dialogService = dialogService;
        _anneeTexte = anneeActuelle.ToString();

        EnregistrerAnneeCommand = new RelayCommand(EnregistrerAnnee);
        AjouterGroupeCommand = new RelayCommand(() => Groupes.Add(new GroupeEditionViewModel("Nouveau groupe")));
        SupprimerGroupeCommand = new RelayCommand<GroupeEditionViewModel>(groupe =>
        {
            if (groupe is not null)
            {
                Groupes.Remove(groupe);
            }
        });
        EnregistrerPostesCommand = new RelayCommand(EnregistrerPostes);

        ChargerBrouillon();
    }

    private void ChargerBrouillon()
    {
        Groupes.Clear();
        foreach (var groupe in _postesRepository.Charger())
        {
            var edition = new GroupeEditionViewModel(groupe.Nom);
            foreach (var poste in groupe.Postes)
            {
                edition.Postes.Add(new PosteEditionViewModel(poste));
            }
            Groupes.Add(edition);
        }
    }

    private void EnregistrerAnnee()
    {
        if (!int.TryParse(AnneeTexte.Trim(), out var annee) || annee < 2000 || annee > 2100)
        {
            ErreurAnnee = "Année invalide (attendu : un nombre entre 2000 et 2100).";
            return;
        }

        ErreurAnnee = null;
        _configRepository.Enregistrer(new Config { AnneeCourante = annee });
        AnneeEnregistree?.Invoke(annee);
    }

    private void EnregistrerPostes()
    {
        try
        {
            var groupes = Groupes
                .Select(g => new GroupePostes
                {
                    Nom = g.Nom.Trim(),
                    Postes = g.Postes.Select(p => p.Nom.Trim()).Where(nom => nom.Length > 0).ToList(),
                })
                .Where(g => g.Nom.Length > 0)
                .ToList();

            _postesRepository.Enregistrer(groupes);
            ChargerBrouillon();

            _dialogService.ShowConfirmation(new ConfirmationModalViewModel("Postes enregistrés", "Les groupes et postes ont bien été enregistrés.", estSucces: true));
        }
        catch (Exception ex)
        {
            _dialogService.ShowConfirmation(new ConfirmationModalViewModel("Échec de l'enregistrement", ex.Message, estSucces: false));
        }
    }
}
