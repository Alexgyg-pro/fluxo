using Fluxo.Domain.Budget;
using Fluxo.Domain.Models;

namespace Fluxo.Domain.Services;

/// <summary>Port de budget.ts.</summary>
public static class BudgetService
{
    public static IReadOnlyList<TypeMois> CalculerTypesMois(int annee, DateOnly aujourdhui)
    {
        var anneeCourante = aujourdhui.Year;
        if (annee < anneeCourante) return Enumerable.Repeat(TypeMois.Reel, 12).ToList();
        if (annee > anneeCourante) return Enumerable.Repeat(TypeMois.Projete, 12).ToList();

        var moisCourant = aujourdhui.Month - 1;
        return Enumerable.Range(0, 12).Select(m => m <= moisCourant ? TypeMois.Reel : TypeMois.Projete).ToList();
    }

    public static IReadOnlyList<int> CalculerMoisClos(int annee, DateOnly aujourdhui)
    {
        var anneeCourante = aujourdhui.Year;
        if (annee < anneeCourante) return Enumerable.Range(0, 12).ToList();
        if (annee > anneeCourante) return Array.Empty<int>();

        var moisCourant = aujourdhui.Month - 1;
        return Enumerable.Range(0, moisCourant).ToList();
    }

    public static BudgetResult CalculerBudget(
        int annee,
        IReadOnlyList<Achat> achatsAnnee,
        IReadOnlyList<Achat> achatsHistorique,
        IReadOnlyList<GroupePostes> groupes,
        DateOnly? aujourdhui = null)
    {
        var today = aujourdhui ?? DateOnly.FromDateTime(DateTime.Now);
        var typesMois = CalculerTypesMois(annee, today);
        var moisClos = CalculerMoisClos(annee, today).ToHashSet();
        var moisProjetes = Enumerable.Range(0, 12).Where(m => typesMois[m] == TypeMois.Projete).ToList();

        var reelParPoste = new Dictionary<string, decimal[]>();
        var moisRenseignesParPoste = new Dictionary<string, HashSet<int>>();

        foreach (var achat in achatsAnnee)
        {
            var mois = achat.DateAchat.Month - 1;
            foreach (var (poste, montant) in achat.Postes)
            {
                if (!reelParPoste.TryGetValue(poste, out var mensuel))
                {
                    mensuel = new decimal[12];
                    reelParPoste[poste] = mensuel;
                }
                mensuel[mois] += montant;

                if (!moisRenseignesParPoste.TryGetValue(poste, out var moisRenseignes))
                {
                    moisRenseignes = new HashSet<int>();
                    moisRenseignesParPoste[poste] = moisRenseignes;
                }
                moisRenseignes.Add(mois);
            }
        }

        LigneBudget LigneBudgetDe(string poste)
        {
            var reel = reelParPoste.TryGetValue(poste, out var r) ? r : new decimal[12];
            var moisRenseignes = moisRenseignesParPoste.TryGetValue(poste, out var mr) ? mr : new HashSet<int>();
            var mensuel = reel.Select(Arrondi.Arrondi2).ToArray();

            var moisClosRenseignes = moisClos.Intersect(moisRenseignes).ToList();
            if (moisClosRenseignes.Count > 0 && moisProjetes.Count > 0)
            {
                var moyenne = Arrondi.Arrondi2(SommeArrondie(moisClosRenseignes.Select(m => reel[m])) / moisClosRenseignes.Count);
                foreach (var m in moisProjetes)
                {
                    mensuel[m] = moyenne;
                }
            }

            return new LigneBudget
            {
                Poste = poste,
                Mensuel = mensuel,
                Total = SommeArrondie(mensuel),
            };
        }

        var groupesBudget = new List<GroupeBudget>();
        foreach (var groupe in groupes)
        {
            var lignes = groupe.Postes.Select(LigneBudgetDe).ToList();
            var totalMensuel = new decimal[12];
            for (var m = 0; m < 12; m++)
            {
                totalMensuel[m] = SommeArrondie(lignes.Select(l => l.Mensuel[m]));
            }

            groupesBudget.Add(new GroupeBudget
            {
                Nom = groupe.Nom,
                Lignes = lignes,
                TotalMensuel = totalMensuel,
                Total = SommeArrondie(totalMensuel),
            });
        }

        var soldeMensuel = new decimal[12];
        for (var m = 0; m < 12; m++)
        {
            soldeMensuel[m] = SommeArrondie(groupesBudget.Select(g =>
                (g.Nom == PostesSpeciaux.GroupeRevenus ? 1m : -1m) * g.TotalMensuel[m]));
        }

        return new BudgetResult
        {
            Annee = annee,
            TypesMois = typesMois,
            Groupes = groupesBudget,
            SoldeMensuel = soldeMensuel,
            SoldeTotal = SommeArrondie(soldeMensuel),
            SoldeEpargne = CalculerSoldeEpargne(achatsHistorique),
            SoldeTicketsRestaurant = CalculerSoldeTicketsRestaurant(achatsHistorique),
        };
    }

    private static decimal CalculerSoldeEpargne(IReadOnlyList<Achat> achats)
    {
        var depots = achats.Sum(a => a.Postes.GetValueOrDefault(PostesSpeciaux.EpargneDepot));
        var retraits = achats.Sum(a => a.Postes.GetValueOrDefault(PostesSpeciaux.EpargneRetrait));
        return Arrondi.Arrondi2(depots - retraits);
    }

    private static decimal CalculerSoldeTicketsRestaurant(IReadOnlyList<Achat> achats)
    {
        var recharges = achats.Sum(a => a.Postes.GetValueOrDefault(PostesSpeciaux.TicketsRestaurant));
        var depenses = achats.Where(a => a.Compte == Compte.TicketRestaurant).Sum(a => a.MontantAchat);
        return Arrondi.Arrondi2(recharges - depenses);
    }

    /// <summary>Accumule une somme en arrondissant à 2 décimales après chaque addition,
    /// pour reproduire fidèlement l'accumulation d'arrondis du TS (pas un arrondi final unique).</summary>
    private static decimal SommeArrondie(IEnumerable<decimal> valeurs)
    {
        var acc = 0m;
        foreach (var v in valeurs)
        {
            acc = Arrondi.Arrondi2(acc + v);
        }
        return acc;
    }
}
