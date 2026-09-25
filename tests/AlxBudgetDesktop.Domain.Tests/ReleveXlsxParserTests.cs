using AlxBudgetDesktop.Domain.Csv;
using Xunit;

namespace AlxBudgetDesktop.Domain.Tests;

public class ReleveXlsxParserTests
{
    private static LigneXlsxBrute Ligne(string? date, string? operation, string? detail, decimal? debit, decimal? credit)
        => new() { Date = date, Operation = operation, Detail = detail, Debit = debit, Credit = credit };

    [Fact]
    public void Ignore_les_lignes_de_synthese_ouverture_cloture_et_total()
    {
        var lignes = new[]
        {
            Ligne("2026-08-03", "SOLDE CRÉDITEUR (ouverture)", null, null, 1000m),
            Ligne("2026-08-05", "PAIEMENT PSC 0508 TRAPPES", "MARKET TRAPPES CARTE 5299", 12.50m, null),
            Ligne(null, "TOTAL DES MOUVEMENTS", null, 12.50m, 0m),
            Ligne("2026-09-01", "SOLDE CRÉDITEUR (clôture)", null, null, 987.50m),
        };

        var resultat = ReleveXlsxParser.Parse(lignes);

        var ligne = Assert.Single(resultat);
        Assert.Equal(new DateOnly(2026, 8, 5), ligne.DatePrelevement);
    }

    [Fact]
    public void Calcule_le_montant_a_partir_du_debit_ou_du_credit()
    {
        var lignes = new[]
        {
            Ligne("2026-08-10", "PRLV SEPA EDF CLIENTS PARTICULI", "GABRIEL NUME", 52.29m, null),
            Ligne("2026-08-28", "VIR EVERIENCE - HELPLINE", "SALAIRE DE 2026-08", null, 1565.77m),
        };

        var resultat = ReleveXlsxParser.Parse(lignes);

        Assert.Equal(2, resultat.Count);
        Assert.Equal(-52.29m, resultat[0].Montant);
        Assert.Equal(1565.77m, resultat[1].Montant);
    }

    [Fact]
    public void Paiement_carte_prend_la_raison_sociale_dans_le_detail_sans_dupliquer_la_ville()
    {
        var lignes = new[]
        {
            Ligne("2026-08-03", "PAIEMENT PSC 0108 TRAPPES", "MARKET TRAPPES CARTE 5299", 17.60m, null),
        };

        var ligne = Assert.Single(ReleveXlsxParser.Parse(lignes));

        Assert.Equal("MARKET TRAPPES", ligne.RaisonSociale);
    }

    [Fact]
    public void Prelevement_sepa_prend_la_raison_sociale_dans_l_operation_pas_dans_le_detail()
    {
        var lignes = new[]
        {
            Ligne("2026-08-10", "PRLV SEPA LES RESIDENCES", "LOYER:202607", 650.22m, null),
        };

        var ligne = Assert.Single(ReleveXlsxParser.Parse(lignes));

        Assert.Equal("LES RESIDENCES", ligne.RaisonSociale);
    }

    [Fact]
    public void Fonctionne_sans_detail_en_se_rabattant_sur_operation_seule()
    {
        var lignes = new[]
        {
            Ligne("2026-08-31", "PRLV SEPA BOUYGUES TELECOM", null, 36.99m, null),
        };

        var ligne = Assert.Single(ReleveXlsxParser.Parse(lignes));

        Assert.Equal("BOUYGUES TELECOM", ligne.RaisonSociale);
    }

    [Fact]
    public void Ignore_les_lignes_sans_date_iso_valide()
    {
        var lignes = new[]
        {
            Ligne("03/08/2026", "PAIEMENT PSC TEST", "TEST", 10m, null),
            Ligne(null, "PAIEMENT PSC TEST 2", "TEST", 10m, null),
            Ligne("2026-08-15", "PAIEMENT PSC VALIDE", "VALIDE", 20m, null),
        };

        var ligne = Assert.Single(ReleveXlsxParser.Parse(lignes));

        Assert.Equal(new DateOnly(2026, 8, 15), ligne.DatePrelevement);
    }
}
