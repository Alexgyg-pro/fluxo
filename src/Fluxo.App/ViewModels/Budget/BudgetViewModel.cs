using System.Collections.ObjectModel;
using Fluxo.App.ViewModels.Shell;
using Fluxo.Data.Repositories;
using Fluxo.Domain.Budget;
using Fluxo.Domain.Services;

namespace Fluxo.App.ViewModels.Budget;

/// <summary>
/// Onglet Budget : tableau annuel en lecture seule (groupes puis postes en lignes,
/// 12 mois + Total en colonnes), alimenté par BudgetService.CalculerBudget (déjà
/// testé côté Domain). Les cellules des mois projetés sont signalées par un fond
/// orange (cf. BudgetView.xaml, un booléen par mois exposé ci-dessous).
/// </summary>
public sealed class BudgetViewModel : TabViewModelBase
{
    private readonly int _annee;
    private readonly PostesRepository _postesRepository;
    private readonly AchatsRepository _achatsRepository;

    public ObservableCollection<LigneBudgetAffichage> Lignes { get; } = [];

    private decimal _soldeCourantCumul;
    public decimal SoldeCourantCumul { get => _soldeCourantCumul; private set => SetProperty(ref _soldeCourantCumul, value); }

    private decimal _soldeEpargneCumul;
    public decimal SoldeEpargneCumul { get => _soldeEpargneCumul; private set => SetProperty(ref _soldeEpargneCumul, value); }

    private decimal _soldeTicketsRestaurantCumul;
    public decimal SoldeTicketsRestaurantCumul { get => _soldeTicketsRestaurantCumul; private set => SetProperty(ref _soldeTicketsRestaurantCumul, value); }

    // Un booléen par mois plutôt qu'une collection indexée : DataGridColumn n'a pas
    // de DataContext (hors de l'arbre visuel), donc chaque colonne du XAML référence
    // directement la propriété du mois qui la concerne via RelativeSource=DataGrid.
    // Non notifiants (private set simple) : ces valeurs ne dépendent que de l'année
    // gérée et de la date du jour, constantes le temps d'une session — inutile de les
    // rendre observables pour le seul rafraîchissement au changement d'onglet.
    public bool Mois0EstProjete { get; private set; }
    public bool Mois1EstProjete { get; private set; }
    public bool Mois2EstProjete { get; private set; }
    public bool Mois3EstProjete { get; private set; }
    public bool Mois4EstProjete { get; private set; }
    public bool Mois5EstProjete { get; private set; }
    public bool Mois6EstProjete { get; private set; }
    public bool Mois7EstProjete { get; private set; }
    public bool Mois8EstProjete { get; private set; }
    public bool Mois9EstProjete { get; private set; }
    public bool Mois10EstProjete { get; private set; }
    public bool Mois11EstProjete { get; private set; }

    public BudgetViewModel(int annee, PostesRepository postesRepository, AchatsRepository achatsRepository)
        : base("📊 Budget")
    {
        _annee = annee;
        _postesRepository = postesRepository;
        _achatsRepository = achatsRepository;

        Rafraichir();
    }

    /// <summary>Recalcule le tableau — appelé à la construction et à chaque fois que
    /// l'onglet redevient actif (MainWindowViewModel), pour refléter les saisies faites
    /// depuis les onglets mois sans avoir à relancer l'application.</summary>
    public void Rafraichir()
    {
        var groupes = _postesRepository.Charger();
        var achatsAnnee = _achatsRepository.ListerAchats(_annee);
        var achatsHistorique = _achatsRepository.ListerTousAchats();

        var budget = BudgetService.CalculerBudget(_annee, achatsAnnee, achatsHistorique, groupes);

        Mois0EstProjete = budget.TypesMois[0] == TypeMois.Projete;
        Mois1EstProjete = budget.TypesMois[1] == TypeMois.Projete;
        Mois2EstProjete = budget.TypesMois[2] == TypeMois.Projete;
        Mois3EstProjete = budget.TypesMois[3] == TypeMois.Projete;
        Mois4EstProjete = budget.TypesMois[4] == TypeMois.Projete;
        Mois5EstProjete = budget.TypesMois[5] == TypeMois.Projete;
        Mois6EstProjete = budget.TypesMois[6] == TypeMois.Projete;
        Mois7EstProjete = budget.TypesMois[7] == TypeMois.Projete;
        Mois8EstProjete = budget.TypesMois[8] == TypeMois.Projete;
        Mois9EstProjete = budget.TypesMois[9] == TypeMois.Projete;
        Mois10EstProjete = budget.TypesMois[10] == TypeMois.Projete;
        Mois11EstProjete = budget.TypesMois[11] == TypeMois.Projete;

        Lignes.Clear();
        foreach (var groupe in budget.Groupes)
        {
            Lignes.Add(LigneBudgetAffichage.PourGroupe(groupe));
            foreach (var ligne in groupe.Lignes)
            {
                Lignes.Add(LigneBudgetAffichage.PourPoste(ligne));
            }
        }
        Lignes.Add(LigneBudgetAffichage.PourSolde(budget.SoldeMensuel, budget.SoldeTotal));

        // Le compte courant cumulé n'est pas fourni par BudgetResult (alxBudget ne
        // l'affichait pas non plus) : on réutilise directement MouvementsMois sur tout
        // l'historique — même formule que le cumul épargne/TR, juste appliquée au
        // mouvement "courant" plutôt qu'à un poste dédié.
        var recettes = SoldesService.PostesRecette(groupes);
        SoldeCourantCumul = SoldesService.MouvementsMois(achatsHistorique, recettes).Courant;
        SoldeEpargneCumul = budget.SoldeEpargne;
        SoldeTicketsRestaurantCumul = budget.SoldeTicketsRestaurant;
    }
}
