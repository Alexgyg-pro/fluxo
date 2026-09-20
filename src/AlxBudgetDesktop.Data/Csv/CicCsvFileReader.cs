using System.Text;

namespace AlxBudgetDesktop.Data.Csv;

/// <summary>
/// Décode un export CSV CIC en Windows-1252 — l'encodage réel de ces fichiers,
/// jamais UTF-8. Nécessite le package System.Text.Encoding.CodePages, enregistré
/// via <see cref="EnregistrerEncodages"/> au démarrage de l'application.
/// </summary>
public static class CicCsvFileReader
{
    private static bool _encodagesEnregistres;

    public static void EnregistrerEncodages()
    {
        if (_encodagesEnregistres)
        {
            return;
        }

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        _encodagesEnregistres = true;
    }

    public static string LireTexte(byte[] contenu)
    {
        EnregistrerEncodages();
        var windows1252 = Encoding.GetEncoding(1252);
        return windows1252.GetString(contenu);
    }

    public static string LireTexte(string cheminFichier)
        => LireTexte(File.ReadAllBytes(cheminFichier));
}
