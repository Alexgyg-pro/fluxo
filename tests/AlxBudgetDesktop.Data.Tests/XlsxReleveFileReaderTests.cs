using AlxBudgetDesktop.Data.Xlsx;
using ClosedXML.Excel;
using Xunit;

namespace AlxBudgetDesktop.Data.Tests;

/// <summary>Vérifie la lecture de l'onglet "Compte courant" sur un classeur
/// synthétique construit en mémoire — jamais un vrai relevé bancaire.</summary>
public class XlsxReleveFileReaderTests
{
    private static Stream ClasseurSynthetique()
    {
        using var classeur = new XLWorkbook();

        var resume = classeur.Worksheets.Add("Résumé");
        resume.Cell(1, 1).Value = "Relevé bancaire — CIC Iberbanco";

        var compteCourant = classeur.Worksheets.Add("Compte courant");
        var enTetes = new[] { "Date", "Date valeur", "Opération", "Détail", "Débit (EUR)", "Crédit (EUR)" };
        for (var i = 0; i < enTetes.Length; i++)
        {
            compteCourant.Cell(1, i + 1).Value = enTetes[i];
        }

        compteCourant.Cell(2, 1).Value = "2026-08-03";
        compteCourant.Cell(2, 3).Value = "SOLDE CRÉDITEUR (ouverture)";
        compteCourant.Cell(2, 6).Value = 1000m;

        compteCourant.Cell(3, 1).Value = "2026-08-05";
        compteCourant.Cell(3, 2).Value = "2026-08-05";
        compteCourant.Cell(3, 3).Value = "PAIEMENT PSC 0508 TRAPPES";
        compteCourant.Cell(3, 4).Value = "MARKET TRAPPES CARTE 5299";
        compteCourant.Cell(3, 5).Value = 12.50m;

        compteCourant.Cell(4, 1).Value = "2026-08-10";
        compteCourant.Cell(4, 2).Value = "2026-08-10";
        compteCourant.Cell(4, 3).Value = "VIR EVERIENCE - HELPLINE";
        compteCourant.Cell(4, 4).Value = "SALAIRE";
        compteCourant.Cell(4, 6).Value = 987.50m;

        compteCourant.Cell(5, 3).Value = "TOTAL DES MOUVEMENTS";
        compteCourant.Cell(5, 5).Value = 12.50m;
        compteCourant.Cell(5, 6).Value = 987.50m;

        compteCourant.Cell(6, 1).Value = "2026-09-01";
        compteCourant.Cell(6, 3).Value = "SOLDE CRÉDITEUR (clôture)";
        compteCourant.Cell(6, 6).Value = 1975.00m;

        var livretDds = classeur.Worksheets.Add("Livret DDS");
        livretDds.Cell(1, 1).Value = "Date";

        var flux = new MemoryStream();
        classeur.SaveAs(flux);
        flux.Position = 0;
        return flux;
    }

    [Fact]
    public void Lit_toutes_les_lignes_de_l_onglet_compte_courant_sans_filtrage()
    {
        // Le filtrage des lignes de synthèse (ouverture/clôture/total) est la
        // responsabilité de ReleveXlsxParser côté Domain, pas de ce lecteur —
        // voir ReleveXlsxParserTests. Ici on vérifie seulement la tabulation brute.
        using var flux = ClasseurSynthetique();

        var lignes = XlsxReleveFileReader.Lire(flux);

        Assert.Equal(5, lignes.Count);
        Assert.Equal("SOLDE CRÉDITEUR (ouverture)", lignes[0].Operation);
        Assert.Equal("PAIEMENT PSC 0508 TRAPPES", lignes[1].Operation);
        Assert.Equal("MARKET TRAPPES CARTE 5299", lignes[1].Detail);
        Assert.Equal(12.50m, lignes[1].Debit);
        Assert.Null(lignes[1].Credit);
        Assert.Equal(987.50m, lignes[2].Credit);
        Assert.Equal("TOTAL DES MOUVEMENTS", lignes[3].Operation);
        Assert.Equal("SOLDE CRÉDITEUR (clôture)", lignes[4].Operation);
    }

    [Fact]
    public void Leve_une_erreur_claire_si_l_onglet_compte_courant_est_absent()
    {
        using var classeur = new XLWorkbook();
        classeur.Worksheets.Add("Autre chose");
        var flux = new MemoryStream();
        classeur.SaveAs(flux);
        flux.Position = 0;

        var exception = Assert.Throws<InvalidOperationException>(() => XlsxReleveFileReader.Lire(flux));
        Assert.Contains("Compte courant", exception.Message);
    }
}
