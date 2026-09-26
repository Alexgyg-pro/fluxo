using System.Text.RegularExpressions;
using AlxBudgetDesktop.Data.Json;
using AlxBudgetDesktop.Data.Paths;
using AlxBudgetDesktop.Domain.Models;
using AlxBudgetDesktop.Domain.Rapprochement;

namespace AlxBudgetDesktop.Data.Repositories;

public sealed partial class AchatsRepository(DataPathProvider chemins, JsonFileStore store)
{
    private string Chemin(int annee) => chemins.CheminFichier($"achats-{annee}.jsonl");

    [GeneratedRegex(@"^achats-(\d{4})\.jsonl$")]
    private static partial Regex NomFichierAnnee();

    /// <summary>Années pour lesquelles un fichier achats-&lt;annee&gt;.jsonl existe déjà sur disque.</summary>
    public List<int> ListerAnnees()
    {
        if (!Directory.Exists(chemins.DossierDonnees))
        {
            return [];
        }

        return Directory.GetFiles(chemins.DossierDonnees, "achats-*.jsonl")
            .Select(f => NomFichierAnnee().Match(Path.GetFileName(f)))
            .Where(m => m.Success)
            .Select(m => int.Parse(m.Groups[1].Value))
            .OrderBy(a => a)
            .ToList();
    }

    public bool ExisteFichier(int annee) => File.Exists(Chemin(annee));

    public List<Achat> ListerAchats(int annee) => store.LoadOrCreateJsonl<Achat>(Chemin(annee));

    /// <summary>Concatène les achats de toutes les années connues — utilisé pour les cumuls historiques (épargne, TR).</summary>
    public List<Achat> ListerTousAchats() => ListerAnnees().SelectMany(ListerAchats).ToList();

    public Achat Ajouter(AchatInput input)
    {
        var achat = new Achat
        {
            Id = Guid.NewGuid(),
            Rapproche = false,
            DateAchat = input.DateAchat,
            Etiquette = input.Etiquette,
            DatePrelevement = input.DatePrelevement,
            RaisonSociale = input.RaisonSociale,
            MontantAchat = input.MontantAchat,
            Compte = input.Compte,
            Postes = input.Postes,
            LibelleReleve = input.LibelleReleve,
        };

        var annee = input.DateAchat.Year;
        var achats = ListerAchats(annee);
        achats.Add(achat);
        store.SaveJsonlAtomic(Chemin(annee), achats);

        return achat;
    }

    /// <summary>
    /// Reproduit fidèlement le comportement d'alxBudget : la recherche de l'achat à modifier
    /// se fait dans le fichier de l'année du <b>nouveau</b> dateAchat, pas l'ancien — si l'édition
    /// change l'année, l'achat doit déjà avoir été déplacé au préalable (limite connue, reprise telle quelle).
    /// </summary>
    public Achat Modifier(Guid id, AchatInput input)
    {
        var annee = input.DateAchat.Year;
        var achats = ListerAchats(annee);
        var index = achats.FindIndex(a => a.Id == id);
        if (index < 0)
        {
            throw new KeyNotFoundException($"Achat {id} introuvable dans achats-{annee}.jsonl.");
        }

        var existant = achats[index];
        var modifie = new Achat
        {
            Id = existant.Id,
            Rapproche = existant.Rapproche,
            DateAchat = input.DateAchat,
            Etiquette = input.Etiquette,
            DatePrelevement = input.DatePrelevement,
            RaisonSociale = input.RaisonSociale,
            MontantAchat = input.MontantAchat,
            Compte = input.Compte,
            Postes = input.Postes,
            LibelleReleve = input.LibelleReleve,
        };

        achats[index] = modifie;
        store.SaveJsonlAtomic(Chemin(annee), achats);

        return modifie;
    }

    public bool Supprimer(int annee, Guid id)
    {
        var achats = ListerAchats(annee);
        var supprime = achats.RemoveAll(a => a.Id == id) > 0;
        if (supprime)
        {
            store.SaveJsonlAtomic(Chemin(annee), achats);
        }

        return supprime;
    }

    /// <summary>Applique les rapprochements confirmés : marque rapproche=true et complète les
    /// infos bancaires (raisonSociale seulement si vide côté saisie manuelle).</summary>
    public void AppliquerRapprochements(int annee, IReadOnlyList<RapprochementMatch> matches)
    {
        var achats = ListerAchats(annee);
        var parId = matches.ToDictionary(m => m.AchatId, m => m.Ligne);

        for (var i = 0; i < achats.Count; i++)
        {
            if (!parId.TryGetValue(achats[i].Id, out var ligne))
            {
                continue;
            }

            var a = achats[i];
            achats[i] = new Achat
            {
                Id = a.Id,
                Rapproche = true,
                DateAchat = a.DateAchat,
                Etiquette = a.Etiquette,
                DatePrelevement = ligne.DatePrelevement,
                RaisonSociale = string.IsNullOrWhiteSpace(a.RaisonSociale) ? ligne.RaisonSociale : a.RaisonSociale,
                MontantAchat = a.MontantAchat,
                Compte = a.Compte,
                Postes = a.Postes,
                LibelleReleve = ligne.LibelleBrut,
            };
        }

        store.SaveJsonlAtomic(Chemin(annee), achats);
    }

    /// <summary>Annule le rapprochement (remet rapproche=false) des achats donnés — permet
    /// de recommencer le contrôle d'un mois en cas d'erreur, sans perdre les achats
    /// eux-mêmes ni leurs autres champs (dateAchat, postes, etc.).</summary>
    public void ReinitialiserRapprochement(int annee, IReadOnlyList<Guid> ids)
    {
        var idsAReinitialiser = ids.ToHashSet();
        var achats = ListerAchats(annee);

        for (var i = 0; i < achats.Count; i++)
        {
            if (!idsAReinitialiser.Contains(achats[i].Id))
            {
                continue;
            }

            var a = achats[i];
            achats[i] = new Achat
            {
                Id = a.Id,
                Rapproche = false,
                DateAchat = a.DateAchat,
                Etiquette = a.Etiquette,
                DatePrelevement = a.DatePrelevement,
                RaisonSociale = a.RaisonSociale,
                MontantAchat = a.MontantAchat,
                Compte = a.Compte,
                Postes = a.Postes,
                LibelleReleve = a.LibelleReleve,
            };
        }

        store.SaveJsonlAtomic(Chemin(annee), achats);
    }
}
