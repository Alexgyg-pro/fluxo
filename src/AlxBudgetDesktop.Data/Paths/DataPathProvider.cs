namespace AlxBudgetDesktop.Data.Paths;

/// <summary>
/// Résout le dossier de données utilisé par l'application. Priorité :
/// 1. Variable d'environnement ALXBUDGET_DESKTOP_DATA_DIR
/// 2. Chemin fourni explicitement au constructeur
/// 3. Dossier "sample-data" à la racine du dépôt, si on le détecte (présence d'un
///    .sln en remontant depuis l'exécutable) — le cas normal en développement
///    (dotnet run), pour que les données survivent aux recompilations.
/// 4. %LOCALAPPDATA%\alxBudgetDesktop\sample-data (cas d'un exécutable publié, hors
///    dépôt, où aucun .sln n'est trouvable — ex. installé dans Program Files). Ne
///    JAMAIS retomber sur "à côté de l'exécutable" dans ce cas : Program Files est
///    protégé en écriture pour un utilisateur standard, l'appli planterait au
///    premier lancement si on essayait d'y créer un dossier.
///
/// Aucune valeur par défaut ne doit jamais référencer le dossier data\ réel
/// d'alxBudget — les données réelles ne sont pointées qu'après validation
/// explicite du PO à l'usage, jamais par une valeur de repli codée en dur ici.
/// </summary>
public sealed class DataPathProvider
{
    private const string EnvVarName = "ALXBUDGET_DESKTOP_DATA_DIR";

    public string DossierDonnees { get; }

    public DataPathProvider(string? cheminConfigure = null)
    {
        var cheminEnv = Environment.GetEnvironmentVariable(EnvVarName);
        var racineDepot = TrouverRacineDepot(AppContext.BaseDirectory);

        DossierDonnees = !string.IsNullOrWhiteSpace(cheminEnv)
            ? cheminEnv
            : !string.IsNullOrWhiteSpace(cheminConfigure)
                ? cheminConfigure
                : racineDepot is not null
                    ? Path.Combine(racineDepot, "sample-data")
                    : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "alxBudgetDesktop", "sample-data");

        DossierDonnees = Path.GetFullPath(DossierDonnees);
        Directory.CreateDirectory(DossierDonnees);
    }

    public string CheminFichier(string nomFichier) => Path.Combine(DossierDonnees, nomFichier);

    /// <summary>Remonte l'arborescence depuis <paramref name="depart"/> à la recherche
    /// d'un fichier .sln, marqueur de la racine du dépôt. Renvoie null si aucun n'est
    /// trouvé (cas d'un exécutable publié, exécuté hors du dépôt source). Public pour
    /// être réutilisé par CheminAideResolver (même détection dev/publié).</summary>
    public static string? TrouverRacineDepot(string depart)
    {
        var dossier = new DirectoryInfo(depart);
        while (dossier is not null)
        {
            if (dossier.GetFiles("*.sln").Length > 0)
            {
                return dossier.FullName;
            }
            dossier = dossier.Parent;
        }
        return null;
    }
}
