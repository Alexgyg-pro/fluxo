using Fluxo.Domain.Models;
using Fluxo.Domain.Rapprochement;
using Fluxo.Domain.Services;
using Xunit;

namespace Fluxo.Domain.Tests;

public class ReconciliationServiceTests
{
    private static Achat Achat(decimal montant, DateOnly dateAchat, bool rapproche = false, string? libelleReleve = null, DateOnly? datePrelevement = null)
        => new()
        {
            Id = Guid.NewGuid(),
            DateAchat = dateAchat,
            Etiquette = "Test",
            MontantAchat = montant,
            Compte = Compte.Courant,
            Postes = new Dictionary<string, decimal> { ["Alimentation"] = montant },
            Rapproche = rapproche,
            LibelleReleve = libelleReleve,
            DatePrelevement = datePrelevement,
        };

    private static LigneReleve Ligne(int index, decimal montant, DateOnly datePrelevement, string libelle = "PAIEMENT MAGASIN")
        => new() { Index = index, Montant = montant, DatePrelevement = datePrelevement, RaisonSociale = "MAGASIN", LibelleBrut = libelle };

    [Fact]
    public void Un_achat_avec_une_seule_ligne_candidate_est_apparie_automatiquement()
    {
        var achat = Achat(10m, new DateOnly(2025, 6, 1));
        var ligne = Ligne(0, -10m, new DateOnly(2025, 6, 3));

        var resultat = ReconciliationService.Rapprocher([achat], [ligne]);

        var match = Assert.Single(resultat.Matches);
        Assert.Equal(achat.Id, match.AchatId);
        Assert.Empty(resultat.Ambigus);
        Assert.Empty(resultat.AchatsNonRapproches);
    }

    [Fact]
    public void Achat_sans_ligne_correspondante_est_orphelin()
    {
        var achat = Achat(10m, new DateOnly(2025, 6, 1));
        var ligne = Ligne(0, -25m, new DateOnly(2025, 6, 3));

        var resultat = ReconciliationService.Rapprocher([achat], [ligne]);

        Assert.Empty(resultat.Matches);
        var orphelin = Assert.Single(resultat.AchatsNonRapproches);
        Assert.Equal(achat.Id, orphelin.Id);
    }

    [Fact]
    public void Deux_lignes_distinctes_a_egale_distance_creent_une_ambiguite()
    {
        var achat = Achat(10m, new DateOnly(2025, 6, 1));
        // Deux lignes à 2 jours d'écart chacune, mais avec des libellés différents.
        var ligneA = Ligne(0, -10m, new DateOnly(2025, 6, 3), "PAIEMENT BOULANGERIE");
        var ligneB = Ligne(1, -10m, new DateOnly(2025, 6, 3), "PAIEMENT PHARMACIE");

        var resultat = ReconciliationService.Rapprocher([achat], [ligneA, ligneB]);

        Assert.Empty(resultat.Matches);
        var ambiguite = Assert.Single(resultat.Ambigus);
        Assert.Equal(2, ambiguite.Candidates.Count);
    }

    [Fact]
    public void Deux_lignes_identiques_a_egale_distance_sont_appariees_automatiquement_sans_ambiguite()
    {
        var achat = Achat(10m, new DateOnly(2025, 6, 1));
        var ligneA = Ligne(0, -10m, new DateOnly(2025, 6, 3), "PAIEMENT BOULANGERIE");
        var ligneB = Ligne(1, -10m, new DateOnly(2025, 6, 3), "PAIEMENT BOULANGERIE");

        var resultat = ReconciliationService.Rapprocher([achat], [ligneA, ligneB]);

        Assert.Single(resultat.Matches);
        Assert.Empty(resultat.Ambigus);
        // La ligne non consommée reste disponible : elle finit en "non rapprochée".
        Assert.Single(resultat.LignesNonRapprochees);
    }

    [Fact]
    public void Ligne_deja_liee_a_un_achat_existant_nest_pas_reproposee_comme_nouvelle_depense()
    {
        var datePrelevement = new DateOnly(2025, 6, 3);
        var achatDejaRapproche = Achat(
            10m, new DateOnly(2025, 6, 1),
            rapproche: true,
            libelleReleve: "PAIEMENT BOULANGERIE",
            datePrelevement: datePrelevement);

        var ligne = Ligne(0, -10m, datePrelevement, "PAIEMENT BOULANGERIE");

        var resultat = ReconciliationService.Rapprocher([achatDejaRapproche], [ligne]);

        Assert.Empty(resultat.LignesNonRapprochees);
    }

    [Fact]
    public void Ordre_de_traitement_glouton_nest_pas_un_appariement_optimal_global()
    {
        // Deux achats de même montant, candidats aux deux mêmes lignes (donc nombre de
        // candidats égal) : le départage se fait par dateAchat croissante, PAS par le
        // meilleur ajustement individuel. Résultat : achatTot (dateAchat la plus ancienne)
        // réserve la ligne la plus proche de LUI (ligneA, écart 3j), ce qui laisse à
        // achatTard la ligne la plus éloignée (ligneB, écart 2j) alors qu'un appariement
        // optimal global aurait donné ligneA à achatTard (écart 1j) et ligneB à achatTot
        // (écart 4j). Ce test documente ce comportement glouton assumé, pas un bug.
        var achatTot = Achat(10m, new DateOnly(2025, 6, 1));
        var achatTard = Achat(10m, new DateOnly(2025, 6, 3));

        var ligneA = Ligne(0, -10m, new DateOnly(2025, 6, 4), "PAIEMENT A");
        var ligneB = Ligne(1, -10m, new DateOnly(2025, 6, 5), "PAIEMENT B");

        var resultat = ReconciliationService.Rapprocher([achatTot, achatTard], [ligneA, ligneB]);

        Assert.Equal(2, resultat.Matches.Count);
        Assert.Empty(resultat.Ambigus);
        Assert.Equal(ligneA.Index, resultat.Matches.Single(m => m.AchatId == achatTot.Id).Ligne.Index);
        Assert.Equal(ligneB.Index, resultat.Matches.Single(m => m.AchatId == achatTard.Id).Ligne.Index);
    }
}
