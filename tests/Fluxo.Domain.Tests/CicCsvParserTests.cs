using Fluxo.Domain.Csv;
using Xunit;

namespace Fluxo.Domain.Tests;

public class CicCsvParserTests
{
    [Fact]
    public void Parse_ligne_reelle_extrait_date_montant_et_raison_sociale()
    {
        // Fixture inspirée du relevé CIC réel (releve-cic-demo.csv) : deux colonnes
        // "Libellé..." présentes, il faut prendre "Libellé opération", pas "Libellé Compte".
        var csv = "Banque;Libellé Compte;RIB;Date opération;Libellé opération;Montant;Devise\n"
                 + "CIC;MON COMPTE COURANT;30066 110XX XXXXXXXX601 XX;23/06/2026;"
                 + "PAIEMENT PSC 2206 VILLENEUVE SUPERMARCHE DEMO CARTE 5299 KIR082617401620923;-4,950;EUR";

        var lignes = CicCsvParser.Parse(csv);

        var ligne = Assert.Single(lignes);
        Assert.Equal(new DateOnly(2026, 6, 23), ligne.DatePrelevement);
        Assert.Equal(-4.950m, ligne.Montant);
        Assert.Equal("VILLENEUVE SUPERMARCHE DEMO", ligne.RaisonSociale);
        Assert.DoesNotContain("COMPTE COURANT", ligne.LibelleBrut);
    }

    [Fact]
    public void Extraction_raison_sociale_supprime_prefixe_code_date_et_reference_transaction()
    {
        var csv = "Date operation;Libelle operation;Montant\n"
                 + "02/07/2026;PAIEMENT PSC 0207 MONTIGNY LE B CARREFOUR STQUEN CARTE 5299 KIR082618401844460;-26,15";

        var lignes = CicCsvParser.Parse(csv);

        var ligne = Assert.Single(lignes);
        Assert.Equal("MONTIGNY LE B CARREFOUR STQUEN", ligne.RaisonSociale);
    }

    [Fact]
    public void Parse_calcule_le_montant_a_partir_des_colonnes_debit_credit_quand_montant_absent()
    {
        var csv = "Date operation;Libelle operation;Debit;Credit\n"
                 + "10/01/2026;VIR SEPA EMPLOYEUR;;1500,00\n"
                 + "11/01/2026;PRLV SEPA EDF;45,30;";

        var lignes = CicCsvParser.Parse(csv);

        Assert.Equal(2, lignes.Count);
        Assert.Equal(1500.00m, lignes[0].Montant);
        Assert.Equal(-45.30m, lignes[1].Montant);
    }

    [Fact]
    public void Lignes_sans_date_valide_sont_ignorees()
    {
        var csv = "Date operation;Libelle operation;Montant\n"
                 + "date invalide;PAIEMENT TEST;-10,00\n"
                 + "15/01/2026;PAIEMENT VALIDE;-20,00";

        var lignes = CicCsvParser.Parse(csv);

        var ligne = Assert.Single(lignes);
        Assert.Equal(new DateOnly(2026, 1, 15), ligne.DatePrelevement);
    }

    [Fact]
    public void Dates_deja_au_format_iso_sont_acceptees_telles_quelles()
    {
        var csv = "Date operation;Libelle operation;Montant\n"
                 + "2026-01-15;PAIEMENT TEST;-10,00";

        var lignes = CicCsvParser.Parse(csv);

        Assert.Equal(new DateOnly(2026, 1, 15), Assert.Single(lignes).DatePrelevement);
    }
}
