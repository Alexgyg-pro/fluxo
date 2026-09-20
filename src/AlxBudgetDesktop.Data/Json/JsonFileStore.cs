using System.Text;
using System.Text.Json;
using AlxBudgetDesktop.Data.Json.Converters;

namespace AlxBudgetDesktop.Data.Json;

/// <summary>
/// Lecture/écriture de fichiers .json (objet unique, indenté) et .jsonl (une entité
/// par ligne, compact), avec les mêmes conventions qu'alxBudget : écriture atomique
/// (fichier .tmp puis remplacement), UTF-8 sans BOM, auto-création paresseuse d'un
/// fichier absent avec sa valeur par défaut.
/// </summary>
public sealed class JsonFileStore
{
    private static readonly UTF8Encoding Utf8SansBom = new(encoderShouldEmitUTF8Identifier: false);

    public static readonly JsonSerializerOptions OptionsIndentees = CreerOptions(indente: true);
    public static readonly JsonSerializerOptions OptionsCompactes = CreerOptions(indente: false);

    private static JsonSerializerOptions CreerOptions(bool indente) => new()
    {
        WriteIndented = indente,
        Converters =
        {
            new DateOnlyJsonConverter(),
            new NullableDateOnlyEmptyStringConverter(),
            new CompteJsonConverter(),
        },
    };

    public T LoadOrCreateJson<T>(string chemin, Func<T> valeurParDefaut)
    {
        if (!File.Exists(chemin))
        {
            var defaut = valeurParDefaut();
            SaveJsonAtomic(chemin, defaut);
            return defaut;
        }

        var texte = File.ReadAllText(chemin, Encoding.UTF8);
        return JsonSerializer.Deserialize<T>(texte, OptionsIndentees)
               ?? throw new InvalidDataException($"Fichier JSON invalide : {chemin}");
    }

    public void SaveJsonAtomic<T>(string chemin, T valeur)
    {
        var texte = JsonSerializer.Serialize(valeur, OptionsIndentees);
        EcrireAtomique(chemin, texte);
    }

    public List<T> LoadOrCreateJsonl<T>(string chemin)
    {
        if (!File.Exists(chemin))
        {
            EcrireAtomique(chemin, "");
            return new List<T>();
        }

        var lignes = File.ReadAllLines(chemin, Encoding.UTF8);
        return lignes
            .Where(l => l.Length > 0)
            .Select(l => JsonSerializer.Deserialize<T>(l, OptionsCompactes)
                         ?? throw new InvalidDataException($"Ligne JSONL invalide dans {chemin} : {l}"))
            .ToList();
    }

    public void SaveJsonlAtomic<T>(string chemin, IReadOnlyList<T> elements)
    {
        var texte = elements.Count == 0
            ? ""
            : string.Join("\n", elements.Select(e => JsonSerializer.Serialize(e, OptionsCompactes))) + "\n";
        EcrireAtomique(chemin, texte);
    }

    private static void EcrireAtomique(string chemin, string contenu)
    {
        var cheminTmp = chemin + ".tmp";
        File.WriteAllText(cheminTmp, contenu, Utf8SansBom);
        File.Move(cheminTmp, chemin, overwrite: true);
    }
}
