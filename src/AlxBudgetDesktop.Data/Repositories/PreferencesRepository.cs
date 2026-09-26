using AlxBudgetDesktop.Data.Json;
using AlxBudgetDesktop.Data.Paths;

namespace AlxBudgetDesktop.Data.Repositories;

/// <summary>Préférences utilisateur mémorisées d'une session à l'autre (hors données
/// financières) — distinct de fenetre.json (dimensions/position) et extensible à
/// d'autres préférences futures.</summary>
public sealed class PreferencesParametres
{
    /// <summary>Dernier dossier utilisé pour sélectionner un relevé bancaire (CSV/xlsx)
    /// via "Contrôler avec un relevé" — sert de dossier initial la fois suivante.</summary>
    public string? DernierDossierReleve { get; init; }
}

public sealed class PreferencesRepository(DataPathProvider chemins, JsonFileStore store)
{
    private string Chemin => chemins.CheminFichier("preferences.json");

    public PreferencesParametres Charger() => store.LoadOrCreateJson(Chemin, () => new PreferencesParametres());

    public void Enregistrer(PreferencesParametres parametres) => store.SaveJsonAtomic(Chemin, parametres);
}
