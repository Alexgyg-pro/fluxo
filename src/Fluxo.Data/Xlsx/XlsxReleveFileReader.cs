using Fluxo.Domain.Csv;
using ClosedXML.Excel;

namespace Fluxo.Data.Xlsx;

/// <summary>
/// Lit l'onglet "Compte courant" du relevé xlsx extrait via Claude (voir
/// docs/prompt-extraction-releve-cic.md) et produit les lignes brutes
/// consommées par Fluxo.Domain.Csv.ReleveXlsxParser. Toute la
/// dépendance à ClosedXML reste ici — Domain n'en sait rien.
/// </summary>
public static class XlsxReleveFileReader
{
    private const string NomOnglet = "Compte courant";

    public static IReadOnlyList<LigneXlsxBrute> Lire(string cheminFichier)
    {
        using var classeur = new XLWorkbook(cheminFichier);
        return Lire(classeur);
    }

    public static IReadOnlyList<LigneXlsxBrute> Lire(Stream flux)
    {
        using var classeur = new XLWorkbook(flux);
        return Lire(classeur);
    }

    private static IReadOnlyList<LigneXlsxBrute> Lire(XLWorkbook classeur)
    {
        var feuille = classeur.Worksheets
            .FirstOrDefault(f => string.Equals(f.Name.Trim(), NomOnglet, StringComparison.OrdinalIgnoreCase));

        if (feuille is null)
        {
            throw new InvalidOperationException(
                $"Onglet « {NomOnglet} » introuvable dans ce classeur. Vérifie qu'il a bien été généré avec le prompt standard (docs/prompt-extraction-releve-cic.md).");
        }

        var enTete = feuille.Row(1).CellsUsed().ToList();
        var idxDate = TrouverColonne(enTete, "Date");
        var idxOperation = TrouverColonne(enTete, "Opération");
        var idxDetail = TrouverColonne(enTete, "Détail");
        var idxDebit = TrouverColonneContenant(enTete, "Débit");
        var idxCredit = TrouverColonneContenant(enTete, "Crédit");

        if (idxDate is null || idxOperation is null || idxDebit is null || idxCredit is null)
        {
            throw new InvalidOperationException(
                $"Colonnes attendues introuvables dans l'onglet « {NomOnglet} » (Date / Opération / Débit / Crédit).");
        }

        var derniereLigne = feuille.LastRowUsed()?.RowNumber() ?? 1;
        var resultat = new List<LigneXlsxBrute>();

        for (var numeroLigne = 2; numeroLigne <= derniereLigne; numeroLigne++)
        {
            var ligne = feuille.Row(numeroLigne);
            resultat.Add(new LigneXlsxBrute
            {
                Date = TexteCellule(ligne, idxDate.Value),
                Operation = TexteCellule(ligne, idxOperation.Value),
                Detail = idxDetail is not null ? TexteCellule(ligne, idxDetail.Value) : null,
                Debit = MontantCellule(ligne, idxDebit.Value),
                Credit = MontantCellule(ligne, idxCredit.Value),
            });
        }

        return resultat;
    }

    private static int? TrouverColonne(IReadOnlyList<IXLCell> enTete, string nom)
        => enTete.FirstOrDefault(c => string.Equals(c.GetString().Trim(), nom, StringComparison.OrdinalIgnoreCase))
            ?.Address.ColumnNumber;

    private static int? TrouverColonneContenant(IReadOnlyList<IXLCell> enTete, string sousChaine)
        => enTete.FirstOrDefault(c => c.GetString().Contains(sousChaine, StringComparison.OrdinalIgnoreCase))
            ?.Address.ColumnNumber;

    private static string? TexteCellule(IXLRow ligne, int colonne)
    {
        var cellule = ligne.Cell(colonne);
        return cellule.IsEmpty() ? null : cellule.GetString();
    }

    private static decimal? MontantCellule(IXLRow ligne, int colonne)
    {
        var cellule = ligne.Cell(colonne);
        if (cellule.IsEmpty())
        {
            return null;
        }

        return cellule.TryGetValue<decimal>(out var valeur) ? valeur : null;
    }
}
