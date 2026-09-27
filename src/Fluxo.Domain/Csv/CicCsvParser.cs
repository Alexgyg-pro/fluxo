using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Fluxo.Domain.Rapprochement;

namespace Fluxo.Domain.Csv;

/// <summary>
/// Port de csv/cic.ts. Opère sur du texte déjà décodé (le décodage Windows-1252
/// des bytes du fichier se fait dans la couche Data, avant d'appeler ce parseur).
/// Heuristique spécifique aux exports CIC — pas un parseur CSV bancaire universel.
/// </summary>
public static class CicCsvParser
{
    public static IReadOnlyList<LigneReleve> Parse(string contenu)
    {
        var lignesBrutes = contenu
            .Split(["\r\n", "\n"], StringSplitOptions.None)
            .Select(l => l.Trim())
            .Where(l => l.Length > 0)
            .ToList();

        if (lignesBrutes.Count == 0)
        {
            return Array.Empty<LigneReleve>();
        }

        var separateur = DetecterSeparateur(lignesBrutes[0]);
        var toutesCellules = lignesBrutes.Select(l => Decouper(l, separateur)).ToList();

        var enteteNormalise = toutesCellules[0].Select(Normaliser).ToList();
        var idxDate = TrouverColonne(enteteNormalise, "date operation", "date d operation", "date");
        var idxLibelle = TrouverColonne(enteteNormalise, "libelle operation", "libelle simplifie", "nature operation", "libelle");
        var idxMontant = TrouverColonne(enteteNormalise, "montant", "montant operation");
        var idxDebit = TrouverColonne(enteteNormalise, "debit");
        var idxCredit = TrouverColonne(enteteNormalise, "credit");

        var aEntete = idxDate >= 0 || idxMontant >= 0 || idxDebit >= 0;
        var corps = aEntete ? toutesCellules.Skip(1) : toutesCellules;

        var resultat = new List<LigneReleve>();
        var index = 0;

        foreach (var cellules in corps)
        {
            var dateBrute = idxDate >= 0 ? Cellule(cellules, idxDate) : Cellule(cellules, 0);
            var datePrelevement = ParseDate(dateBrute);
            if (datePrelevement is null)
            {
                continue;
            }

            decimal? montant = idxMontant >= 0
                ? ParseMontant(Cellule(cellules, idxMontant))
                : (idxDebit >= 0 || idxCredit >= 0
                    ? Math.Abs(ParseMontant(Cellule(cellules, idxCredit))) - Math.Abs(ParseMontant(Cellule(cellules, idxDebit)))
                    : null);

            if (montant is null)
            {
                continue;
            }

            var libelle = NettoyerLibelle(idxLibelle >= 0 ? Cellule(cellules, idxLibelle) : "");

            resultat.Add(new LigneReleve
            {
                Index = index++,
                DatePrelevement = datePrelevement.Value,
                Montant = montant.Value,
                RaisonSociale = LibelleReleveHeuristiques.ExtraireRaisonSociale(libelle),
                LibelleBrut = libelle,
            });
        }

        return resultat;
    }

    private static string Cellule(IReadOnlyList<string> cellules, int index) => index < cellules.Count ? cellules[index] : "";

    private static char DetecterSeparateur(string premiereLigne)
    {
        var nbPointVirgule = premiereLigne.Count(c => c == ';');
        var nbVirgule = premiereLigne.Count(c => c == ',');
        var nbTab = premiereLigne.Count(c => c == '\t');

        if (nbTab >= nbPointVirgule && nbTab >= nbVirgule) return '\t';
        if (nbPointVirgule >= nbVirgule) return ';';
        return ',';
    }

    private static List<string> Decouper(string ligne, char separateur)
    {
        var cellules = new List<string>();
        var courante = new StringBuilder();
        var dansGuillemets = false;

        foreach (var c in ligne)
        {
            if (c == '"')
            {
                dansGuillemets = !dansGuillemets;
            }
            else if (c == separateur && !dansGuillemets)
            {
                cellules.Add(courante.ToString());
                courante.Clear();
            }
            else
            {
                courante.Append(c);
            }
        }
        cellules.Add(courante.ToString());
        return cellules;
    }

    private static string Normaliser(string s)
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

        var sansApostrophes = sansDiacritiques.ToString().Replace('\'', ' ').Replace('"', ' ');
        return EspacesMultiples.Replace(sansApostrophes, " ").Trim();
    }

    private static int TrouverColonne(IReadOnlyList<string> enteteNormalise, params string[] alias)
    {
        foreach (var a in alias)
        {
            var idx = enteteNormalise.ToList().FindIndex(e => e == a);
            if (idx >= 0) return idx;
        }
        foreach (var a in alias)
        {
            var idx = enteteNormalise.ToList().FindIndex(e => e.Contains(a));
            if (idx >= 0) return idx;
        }
        return -1;
    }

    private static readonly Regex EspacesMultiples = new(@"\s+", RegexOptions.Compiled);
    private static readonly Regex DateIso = new(@"^\d{4}-\d{2}-\d{2}$", RegexOptions.Compiled);
    private static readonly Regex DateJourMoisAnnee = new(@"^(\d{2})/(\d{2})/(\d{4})$", RegexOptions.Compiled);

    private static DateOnly? ParseDate(string brut)
    {
        var s = brut.Trim();
        if (DateIso.IsMatch(s))
        {
            return DateOnly.ParseExact(s, "yyyy-MM-dd", CultureInfo.InvariantCulture);
        }

        var m = DateJourMoisAnnee.Match(s);
        if (m.Success)
        {
            var jour = int.Parse(m.Groups[1].Value);
            var mois = int.Parse(m.Groups[2].Value);
            var annee = int.Parse(m.Groups[3].Value);
            return new DateOnly(annee, mois, jour);
        }

        return null;
    }

    private static decimal ParseMontant(string brut)
    {
        if (string.IsNullOrEmpty(brut)) return 0m;

        var sansEspaces = new string(brut.Where(c => !char.IsWhiteSpace(c) && c != ' ').ToArray());
        var sansMilliers = sansEspaces.Replace(".", "");
        var pointDecimal = sansMilliers.Replace(',', '.');

        return decimal.TryParse(pointDecimal, NumberStyles.Number, CultureInfo.InvariantCulture, out var valeur)
            ? valeur
            : 0m;
    }

    private static string NettoyerLibelle(string s) => EspacesMultiples.Replace(s, " ").Trim();
}
