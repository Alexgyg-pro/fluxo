using System.Globalization;
using Fluxo.Domain.Rapprochement;

namespace Fluxo.Domain.Csv;

/// <summary>
/// Parseur du relevé bancaire extrait en xlsx par Claude à partir du PDF CIC
/// (voir docs/prompt-extraction-releve-cic.md pour le prompt figé qui garantit
/// cette structure). Prend des lignes déjà tabulées (lues par
/// Fluxo.Data.Xlsx.XlsxReleveFileReader) — Domain reste sans
/// dépendance à une bibliothèque xlsx. Produit la même forme de sortie que
/// CicCsvParser, consommée telle quelle par ReconciliationService.
/// </summary>
public static class ReleveXlsxParser
{
    public static IReadOnlyList<LigneReleve> Parse(IReadOnlyList<LigneXlsxBrute> lignesBrutes)
    {
        var resultat = new List<LigneReleve>();
        var index = 0;

        foreach (var brute in lignesBrutes)
        {
            var operationNormalisee = LibelleReleveHeuristiques.Normaliser(brute.Operation ?? "");
            if (EstLigneDeSynthese(operationNormalisee))
            {
                continue;
            }

            var datePrelevement = ParseDateIso(brute.Date);
            if (datePrelevement is null)
            {
                continue;
            }

            var montant = Arrondi.Arrondi2((brute.Credit ?? 0m) - (brute.Debit ?? 0m));
            if (montant == 0m)
            {
                continue;
            }

            var libelle = $"{brute.Operation} {brute.Detail}".Trim();

            resultat.Add(new LigneReleve
            {
                Index = index++,
                DatePrelevement = datePrelevement.Value,
                Montant = montant,
                RaisonSociale = ExtraireRaisonSocialeXlsx(brute.Operation ?? "", brute.Detail ?? ""),
                LibelleBrut = libelle,
            });
        }

        return resultat;
    }

    /// <summary>
    /// Contrairement au CSV CIC officiel (une seule chaîne "Opération" à
    /// nettoyer), le xlsx sépare déjà le préfixe technique ("PAIEMENT PSC 0108
    /// TRAPPES") du libellé commerçant ("MARKET TRAPPES CARTE 5299") — les
    /// concaténer et passer le tout dans ExtraireRaisonSociale duplique le nom
    /// de ville (le code à 4 chiffres qui le précède dans "Opération" n'est
    /// reconnu comme tel que par CodeDateResiduel, pas la ville qui suit).
    /// Pour un paiement carte/retrait, le nom du commerçant est déjà isolé
    /// dans Détail : on ne nettoie que son suffixe "CARTE nnnn" au lieu de
    /// rejouer tout le pipeline sur la concaténation. Pour un prélèvement/
    /// virement, c'est l'inverse : le nom du bénéficiaire est dans Opération
    /// (après le préfixe "PRLV SEPA"/"VIR SEPA"/…), et Détail n'est qu'une
    /// référence de paiement à ignorer (ex. "LOYER:202607").
    /// </summary>
    private static string ExtraireRaisonSocialeXlsx(string operation, string detail)
    {
        var operationMajuscule = operation.ToUpperInvariant();
        var estPaiementOuRetrait = operationMajuscule.StartsWith("PAIEMENT", StringComparison.Ordinal)
            || operationMajuscule.StartsWith("RETRAIT", StringComparison.Ordinal);

        if (estPaiementOuRetrait && !string.IsNullOrWhiteSpace(detail))
        {
            return LibelleReleveHeuristiques.ExtraireRaisonSociale(detail);
        }

        return LibelleReleveHeuristiques.ExtraireRaisonSociale(operation);
    }

    private static bool EstLigneDeSynthese(string operationNormalisee)
        => operationNormalisee.StartsWith("solde crediteur", StringComparison.Ordinal)
           || operationNormalisee.StartsWith("solde debiteur", StringComparison.Ordinal)
           || operationNormalisee == "total des mouvements";

    private static DateOnly? ParseDateIso(string? brut)
        => DateOnly.TryParseExact(brut, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
            ? date
            : null;
}
