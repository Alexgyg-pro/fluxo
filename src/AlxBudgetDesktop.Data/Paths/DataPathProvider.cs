namespace AlxBudgetDesktop.Data.Paths;

/// <summary>
/// Résout le dossier de données utilisé par l'application. Priorité :
/// 1. Variable d'environnement ALXBUDGET_DESKTOP_DATA_DIR
/// 2. Chemin fourni explicitement au constructeur (ex. lu depuis appsettings.json)
/// 3. Dossier "sample-data" à côté de l'exécutable
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

        DossierDonnees = !string.IsNullOrWhiteSpace(cheminEnv)
            ? cheminEnv
            : !string.IsNullOrWhiteSpace(cheminConfigure)
                ? cheminConfigure
                : Path.Combine(AppContext.BaseDirectory, "sample-data");

        DossierDonnees = Path.GetFullPath(DossierDonnees);
        Directory.CreateDirectory(DossierDonnees);
    }

    public string CheminFichier(string nomFichier) => Path.Combine(DossierDonnees, nomFichier);
}
