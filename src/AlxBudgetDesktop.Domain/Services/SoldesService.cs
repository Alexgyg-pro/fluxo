using AlxBudgetDesktop.Domain.Models;

namespace AlxBudgetDesktop.Domain.Services;

/// <summary>Port de soldes.ts.</summary>
public static class SoldesService
{
    public static IReadOnlySet<string> PostesRecette(IReadOnlyList<GroupePostes> groupes)
    {
        var groupe = groupes.FirstOrDefault(g => g.Nom == PostesSpeciaux.GroupeRevenus);
        return groupe is null ? new HashSet<string>() : groupe.Postes.ToHashSet();
    }

    public static SoldesComptes MouvementsMois(IReadOnlyList<Achat> achats, IReadOnlySet<string> recettes)
    {
        decimal courant = 0, ticketRestaurant = 0, epargne = 0;

        foreach (var a in achats)
        {
            var rechargeTr = a.Postes.GetValueOrDefault(PostesSpeciaux.TicketsRestaurant);
            var depot = a.Postes.GetValueOrDefault(PostesSpeciaux.EpargneDepot);
            var retrait = a.Postes.GetValueOrDefault(PostesSpeciaux.EpargneRetrait);
            var estRecette = a.Postes.Keys.Any(recettes.Contains);

            ticketRestaurant += rechargeTr;
            if (a.Compte == Compte.TicketRestaurant)
            {
                ticketRestaurant -= a.MontantAchat;
            }

            epargne += depot - retrait;

            if (a.Compte != Compte.TicketRestaurant)
            {
                courant += estRecette ? a.MontantAchat - rechargeTr : -a.MontantAchat;
            }
        }

        return new SoldesComptes
        {
            Courant = Arrondi.Arrondi2(courant),
            TicketRestaurant = Arrondi.Arrondi2(ticketRestaurant),
            Epargne = Arrondi.Arrondi2(epargne),
        };
    }

    /// <summary>Calcule les 12 mois d'une année. Un calage manuel écrase le report
    /// automatique et se propage en cascade aux mois suivants non calés (le report
    /// du mois M+1 est le disponible du mois M déjà calé).</summary>
    public static SoldesAnnee CalculerSoldesAnnee(
        int annee,
        IReadOnlyList<Achat> achatsAnnee,
        IReadOnlyDictionary<string, SoldesComptes> calages,
        IReadOnlyList<GroupePostes> groupes,
        SoldesComptes? ouvertureJanvier = null)
    {
        var recettes = PostesRecette(groupes);
        var mois = new List<SoldesMois>(12);
        SoldesComptes disponiblePrecedent = ouvertureJanvier ?? SoldesComptes.Zero;

        for (var m = 0; m < 12; m++)
        {
            var report = m == 0 ? (ouvertureJanvier ?? SoldesComptes.Zero) : disponiblePrecedent;
            var cale = calages.TryGetValue(CleMois(annee, m), out var soldesCalage) ? soldesCalage : null;
            var ouverture = cale ?? report;

            var achatsM = achatsAnnee.Where(a => a.DateAchat.Year == annee && a.DateAchat.Month - 1 == m).ToList();
            var mouvements = MouvementsMois(achatsM, recettes);
            var disponible = ouverture + mouvements;

            mois.Add(new SoldesMois
            {
                Mois = m,
                Report = report,
                Ouverture = ouverture,
                Mouvements = mouvements,
                Disponible = disponible,
                Cale = cale is not null,
            });

            disponiblePrecedent = disponible;
        }

        return new SoldesAnnee { Annee = annee, Mois = mois };
    }

    public static string CleMois(int annee, int moisZeroIndexe) => $"{annee:0000}-{moisZeroIndexe + 1:00}";
}
