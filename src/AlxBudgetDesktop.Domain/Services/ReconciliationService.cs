using System.Text.RegularExpressions;
using AlxBudgetDesktop.Domain.Models;
using AlxBudgetDesktop.Domain.Rapprochement;

namespace AlxBudgetDesktop.Domain.Services;

/// <summary>
/// Port de reconciliation.ts. Algorithme glouton (pas un appariement optimal global) :
/// les achats les plus contraints (le moins de candidats) sont traités en premier pour
/// qu'ils réservent leur ligne avant qu'un achat plus ambigu ne la leur prenne.
/// </summary>
public static class ReconciliationService
{
    public static ResultatRapprochement Rapprocher(
        IReadOnlyList<Achat> achatsCourant,
        IReadOnlyList<LigneReleve> lignes,
        IReadOnlyDictionary<string, IReadOnlyList<string>>? beneficiaires = null)
    {
        beneficiaires ??= new Dictionary<string, IReadOnlyList<string>>();

        // Seuls les achats pas encore rapprochés participent au matching.
        var achatsCandidats = achatsCourant.Where(a => !a.Rapproche).ToList();

        var candidatsParAchat = achatsCandidats.ToDictionary(
            a => a.Id,
            a => lignes.Where(l => Correspond(a, l)).ToList());

        var ordreTraitement = achatsCandidats
            .OrderBy(a => candidatsParAchat[a.Id].Count)
            .ThenBy(a => a.DateAchat)
            .ToList();

        var lignesReservees = new HashSet<int>();
        var matches = new List<RapprochementMatch>();
        var ambigus = new List<Ambiguite>();
        var achatsNonRapproches = new List<Achat>();

        foreach (var achat in ordreTraitement)
        {
            var disponibles = candidatsParAchat[achat.Id]
                .Where(l => !lignesReservees.Contains(l.Index))
                .OrderBy(l => EcartJours(achat.DateAchat, l.DatePrelevement))
                .ThenBy(l => l.DatePrelevement)
                .ToList();

            if (disponibles.Count == 0)
            {
                achatsNonRapproches.Add(achat);
                continue;
            }

            var distMin = EcartJours(achat.DateAchat, disponibles[0].DatePrelevement);
            var exAequo = disponibles
                .Where(l => EcartJours(achat.DateAchat, l.DatePrelevement) == distMin)
                .ToList();

            var identitesDistinctes = exAequo.Select(Identite).Distinct().Count();

            if (identitesDistinctes > 1)
            {
                ambigus.Add(new Ambiguite { AchatId = achat.Id, Candidates = exAequo });
                foreach (var l in exAequo)
                {
                    lignesReservees.Add(l.Index);
                }
            }
            else
            {
                matches.Add(new RapprochementMatch { AchatId = achat.Id, Ligne = exAequo[0] });
                lignesReservees.Add(exAequo[0].Index);
            }
        }

        var lignesNonRapprochees = lignes
            .Where(l => !lignesReservees.Contains(l.Index))
            .Where(l => !achatsCourant.Any(a => MemeTransaction(a, l)))
            .Select(l => new LigneNonRapprochee
            {
                Ligne = l,
                PostesSuggeres = beneficiaires.TryGetValue(CleRaisonSociale(l.RaisonSociale), out var postes)
                    ? postes
                    : Array.Empty<string>(),
            })
            .ToList();

        return new ResultatRapprochement
        {
            Matches = matches,
            Ambigus = ambigus,
            LignesNonRapprochees = lignesNonRapprochees,
            AchatsNonRapproches = achatsNonRapproches,
        };
    }

    private static bool Correspond(Achat achat, LigneReleve ligne)
        => Arrondi.Arrondi2(Math.Abs(ligne.Montant)) == Arrondi.Arrondi2(achat.MontantAchat)
           && ligne.DatePrelevement >= achat.DateAchat;

    private static bool MemeTransaction(Achat achat, LigneReleve ligne)
        => achat.LibelleReleve is not null
           && Arrondi.Arrondi2(Math.Abs(ligne.Montant)) == Arrondi.Arrondi2(achat.MontantAchat)
           && achat.DatePrelevement == ligne.DatePrelevement
           && NormaliserLibelle(achat.LibelleReleve) == NormaliserLibelle(ligne.LibelleBrut);

    private static int EcartJours(DateOnly a, DateOnly b) => Math.Abs(a.DayNumber - b.DayNumber);

    private static string Identite(LigneReleve ligne) => $"{ligne.DatePrelevement:yyyy-MM-dd}|{NormaliserLibelle(ligne.LibelleBrut)}";

    private static readonly Regex EspacesMultiples = new(@"\s+", RegexOptions.Compiled);

    private static string NormaliserLibelle(string s) => EspacesMultiples.Replace(s.ToUpperInvariant(), " ").Trim();

    private static string CleRaisonSociale(string raisonSociale) => NormaliserLibelle(raisonSociale);
}
