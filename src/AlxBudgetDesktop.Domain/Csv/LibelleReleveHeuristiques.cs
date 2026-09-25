using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace AlxBudgetDesktop.Domain.Csv;

/// <summary>
/// Heuristiques de nettoyage de libellé partagées entre les parseurs de relevé
/// (CSV CIC officiel et xlsx extrait via Claude) — extraites de CicCsvParser
/// pour être réutilisées telles quelles par ReleveXlsxParser.
/// </summary>
internal static class LibelleReleveHeuristiques
{
    private static readonly Regex EspacesMultiples = new(@"\s+", RegexOptions.Compiled);
    private static readonly Regex SuffixeCarte = new(@"\sCARTE\s+\d.*$", RegexOptions.Compiled);
    private static readonly Regex PrefixeOperation = new(
        @"^(PAIEMENT\s+PSC|PAIEMENT\s+CB|PRLV\s+SEPA|VIR\s+SEPA|RETRAIT\s+DAB|PRLV|VIR)\s+",
        RegexOptions.Compiled);
    private static readonly Regex CodeDateResiduel = new(@"^\d{4}\s+", RegexOptions.Compiled);
    private static readonly Regex ReferenceTransactionResiduelle = new(
        @"\s*[A-Z]?\d{2}[A-Z]\d{3,}[A-Z]?\d*$", RegexOptions.Compiled);

    public static string ExtraireRaisonSociale(string libelle)
    {
        var s = libelle.ToUpperInvariant();
        s = SuffixeCarte.Replace(s, "");
        s = PrefixeOperation.Replace(s, "");
        s = CodeDateResiduel.Replace(s, "");
        s = ReferenceTransactionResiduelle.Replace(s, "");
        return EspacesMultiples.Replace(s, " ").Trim();
    }

    /// <summary>Normalise un libellé pour comparaison insensible à la casse et
    /// aux diacritiques (ex. détecter les lignes de synthèse "SOLDE CRÉDITEUR"
    /// même si un mois l'orthographie différemment).</summary>
    public static string Normaliser(string s)
    {
        var minuscule = s.ToLowerInvariant();
        var formeDecomposee = minuscule.Normalize(NormalizationForm.FormD);
        var sansDiacritiques = new StringBuilder();
        foreach (var c in formeDecomposee)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            {
                sansDiacritiques.Append(c);
            }
        }

        return EspacesMultiples.Replace(sansDiacritiques.ToString(), " ").Trim();
    }
}
