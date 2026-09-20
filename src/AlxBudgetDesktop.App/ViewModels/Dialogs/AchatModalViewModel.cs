using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;
using AlxBudgetDesktop.App.Mvvm;
using AlxBudgetDesktop.Data.Repositories;
using AlxBudgetDesktop.Domain;
using AlxBudgetDesktop.Domain.Models;
using AlxBudgetDesktop.Domain.Services;

namespace AlxBudgetDesktop.App.ViewModels.Dialogs;

/// <summary>
/// Modale de saisie/édition d'une dépense ou d'une recette. Port du composant
/// AchatModal.tsx : répartition multi-postes, indicateur temps réel "somme des
/// postes = montant", gestion du signe des postes Remises, section infos bancaires
/// facultative. Le ViewModel enregistre lui-même l'achat (via AchatsRepository) —
/// l'appelant (MoisViewModel) n'a qu'à rafraîchir sa liste une fois la modale fermée
/// avec un résultat positif.
/// </summary>
public sealed class AchatModalViewModel : ModalViewModelBase
{
    private readonly IReadOnlyList<GroupePostes> _groupes;
    private readonly AchatsRepository _achatsRepository;
    private readonly Achat? _achatExistant;

    public ModeAchat Mode { get; }
    public bool EstEdition => _achatExistant is not null;

    public string Titre => (EstEdition, Mode) switch
    {
        (true, ModeAchat.Recette) => "Modifier la recette",
        (true, ModeAchat.Depense) => "Modifier la dépense",
        (false, ModeAchat.Recette) => "Ajouter une recette",
        (false, ModeAchat.Depense) => "Ajouter une dépense",
        _ => throw new ArgumentOutOfRangeException(nameof(Mode)),
    };

    public bool AfficheSelecteurCompte => Mode == ModeAchat.Depense;

    /// <summary>Vue groupée par nom de groupe (Dépenses incontournables, Dépenses
    /// domestiques, ...) pour l'affichage en liste déroulante avec en-têtes non
    /// cliquables — cf. GroupStyle sur le ComboBox dans AchatModalWindow.xaml.</summary>
    public ICollectionView PostesGroupes { get; }

    public ObservableCollection<LignePosteViewModel> LignesPostes { get; } = [];

    private DateOnly _dateAchat;
    public DateOnly DateAchat
    {
        get => _dateAchat;
        set => SetProperty(ref _dateAchat, value);
    }

    private string _montantTexte = "";
    public string MontantTexte
    {
        get => _montantTexte;
        set
        {
            if (SetProperty(ref _montantTexte, value))
            {
                RecalculerEtat();
            }
        }
    }

    private string _etiquette = "";
    public string Etiquette
    {
        get => _etiquette;
        set
        {
            if (SetProperty(ref _etiquette, value))
            {
                RecalculerEtat();
            }
        }
    }

    private Compte _compte;
    public Compte Compte
    {
        get => _compte;
        set => SetProperty(ref _compte, value);
    }

    private string _datePrelevementTexte = "";
    public string DatePrelevementTexte
    {
        get => _datePrelevementTexte;
        set => SetProperty(ref _datePrelevementTexte, value);
    }

    private string _raisonSociale = "";
    public string RaisonSociale
    {
        get => _raisonSociale;
        set => SetProperty(ref _raisonSociale, value);
    }

    private string? _messageErreur;
    public string? MessageErreur
    {
        get => _messageErreur;
        private set
        {
            if (SetProperty(ref _messageErreur, value))
            {
                OnPropertyChanged(nameof(AMessageErreur));
            }
        }
    }

    public bool AMessageErreur => !string.IsNullOrEmpty(MessageErreur);

    public decimal MontantSaisi => ParseDecimal(MontantTexte);
    public decimal SommePostes => LignesPostes.Sum(l => l.MontantEffectif);
    public bool SommeValide => Arrondi.Arrondi2(SommePostes) == Arrondi.Arrondi2(MontantSaisi);
    public string SommeMessage => $"Somme des postes : {Arrondi.Arrondi2(SommePostes):0.00} € — Montant : {Arrondi.Arrondi2(MontantSaisi):0.00} €";

    public RelayCommand AjouterLignePosteCommand { get; }
    public RelayCommand CompleterLeResteCommand { get; }
    public RelayCommand<LignePosteViewModel> SupprimerLignePosteCommand { get; }
    public RelayCommand EnregistrerCommand { get; }
    public RelayCommand AnnulerCommand { get; }

    public AchatModalViewModel(
        ModeAchat mode,
        IReadOnlyList<GroupePostes> groupes,
        AchatsRepository achatsRepository,
        DateOnly dateParDefaut,
        Achat? achatExistant = null)
    {
        Mode = mode;
        _groupes = groupes;
        _achatsRepository = achatsRepository;
        _achatExistant = achatExistant;

        var groupesPertinents = mode == ModeAchat.Recette
            ? groupes.Where(g => g.Nom == PostesSpeciaux.GroupeRevenus)
            : groupes.Where(g => g.Nom != PostesSpeciaux.GroupeRevenus);

        var postesAvecGroupe = groupesPertinents
            .SelectMany(g => g.Postes.Select(p => new PosteOption { Poste = p, Groupe = g.Nom }))
            .ToList();

        var vueGroupee = CollectionViewSource.GetDefaultView(postesAvecGroupe);
        vueGroupee.GroupDescriptions.Add(new PropertyGroupDescription(nameof(PosteOption.Groupe)));
        PostesGroupes = vueGroupee;

        // Les commandes doivent exister AVANT tout ajout à LignesPostes ci-dessous :
        // CollectionChanged déclenche RecalculerEtat(), qui référence EnregistrerCommand.
        AjouterLignePosteCommand = new RelayCommand(() => LignesPostes.Add(new LignePosteViewModel(groupes)));
        SupprimerLignePosteCommand = new RelayCommand<LignePosteViewModel>(ligne =>
        {
            if (ligne is not null && LignesPostes.Count > 1)
            {
                LignesPostes.Remove(ligne);
            }
        });
        CompleterLeResteCommand = new RelayCommand(CompleterLeReste);
        EnregistrerCommand = new RelayCommand(Enregistrer, PeutEnregistrer);
        AnnulerCommand = new RelayCommand(() => Close(false));

        LignesPostes.CollectionChanged += (_, e) =>
        {
            if (e.NewItems is not null)
            {
                foreach (LignePosteViewModel ligne in e.NewItems)
                {
                    ligne.PropertyChanged += SurChangementLigne;
                }
            }
            if (e.OldItems is not null)
            {
                foreach (LignePosteViewModel ligne in e.OldItems)
                {
                    ligne.PropertyChanged -= SurChangementLigne;
                }
            }
            RecalculerEtat();
        };

        if (achatExistant is not null)
        {
            _dateAchat = achatExistant.DateAchat;
            _montantTexte = achatExistant.MontantAchat.ToString("0.00", CultureInfo.InvariantCulture);
            _etiquette = achatExistant.Etiquette;
            _compte = achatExistant.Compte;
            _datePrelevementTexte = achatExistant.DatePrelevement?.ToString("yyyy-MM-dd") ?? "";
            _raisonSociale = achatExistant.RaisonSociale;

            foreach (var (poste, montant) in achatExistant.Postes)
            {
                var ligne = new LignePosteViewModel(groupes) { PosteSelectionne = poste };
                ligne.MontantTexte = Math.Abs(montant).ToString("0.00", CultureInfo.InvariantCulture);
                LignesPostes.Add(ligne);
            }
        }
        else
        {
            _dateAchat = dateParDefaut;
            _compte = Compte.Courant;
            LignesPostes.Add(new LignePosteViewModel(groupes));
        }
    }

    private void SurChangementLigne(object? sender, PropertyChangedEventArgs e) => RecalculerEtat();

    private void RecalculerEtat()
    {
        OnPropertyChanged(nameof(MontantSaisi));
        OnPropertyChanged(nameof(SommePostes));
        OnPropertyChanged(nameof(SommeValide));
        OnPropertyChanged(nameof(SommeMessage));
        EnregistrerCommand.RaiseCanExecuteChanged();
    }

    private bool PeutEnregistrer()
        => !string.IsNullOrWhiteSpace(Etiquette) && MontantSaisi > 0 && SommeValide;

    private void CompleterLeReste()
    {
        var ligneAvide = LignesPostes.FirstOrDefault(l => l.MontantSaisi == 0);
        if (ligneAvide is null)
        {
            return;
        }

        var autresLignes = LignesPostes.Where(l => l != ligneAvide).Sum(l => l.MontantEffectif);
        var reste = MontantSaisi - autresLignes;
        ligneAvide.MontantTexte = Arrondi.Arrondi2(reste).ToString("0.00", CultureInfo.InvariantCulture);
    }

    private void Enregistrer()
    {
        var postes = new Dictionary<string, decimal>();
        foreach (var ligne in LignesPostes.Where(l => !string.IsNullOrWhiteSpace(l.PosteSelectionne)))
        {
            postes[ligne.PosteSelectionne!] = ligne.MontantEffectif;
        }

        var input = new AchatInput
        {
            DateAchat = DateAchat,
            Etiquette = Etiquette.Trim(),
            DatePrelevement = ParseDateOptionnelle(DatePrelevementTexte),
            RaisonSociale = RaisonSociale,
            MontantAchat = MontantSaisi,
            Compte = Mode == ModeAchat.Recette ? Compte.Courant : Compte,
            Postes = postes,
            LibelleReleve = _achatExistant?.LibelleReleve,
        };

        var erreurs = AchatValidator.Valider(input);
        if (erreurs.Count > 0)
        {
            MessageErreur = string.Join(" ", erreurs);
            return;
        }

        if (_achatExistant is not null)
        {
            _achatsRepository.Modifier(_achatExistant.Id, input);
        }
        else
        {
            _achatsRepository.Ajouter(input);
        }

        Close(true);
    }

    private static decimal ParseDecimal(string texte)
        => decimal.TryParse(texte.Replace(',', '.'), NumberStyles.Number, CultureInfo.InvariantCulture, out var v) ? v : 0m;

    private static DateOnly? ParseDateOptionnelle(string texte)
        => DateOnly.TryParseExact(texte.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d) ? d : null;
}
