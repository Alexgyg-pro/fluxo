namespace AlxBudgetDesktop.Data.Paths;

/// <summary>
/// Résout le chemin du mode d'emploi utilisateur à ouvrir depuis le bouton d'aide :
/// README.txt à côté de l'exécutable (cas publié, copié depuis README-utilisateur.txt
/// à chaque `dotnet publish`), sinon README-utilisateur.txt à la racine du dépôt
/// (cas `dotnet run`, où ce fichier n'est jamais copié à côté des binaires).
/// </summary>
public static class CheminAideResolver
{
    public static string? Resoudre()
    {
        var cheminPublie = Path.Combine(AppContext.BaseDirectory, "README.txt");
        if (File.Exists(cheminPublie))
        {
            return cheminPublie;
        }

        var racineDepot = DataPathProvider.TrouverRacineDepot(AppContext.BaseDirectory);
        if (racineDepot is null)
        {
            return null;
        }

        var cheminDev = Path.Combine(racineDepot, "README-utilisateur.txt");
        return File.Exists(cheminDev) ? cheminDev : null;
    }
}
