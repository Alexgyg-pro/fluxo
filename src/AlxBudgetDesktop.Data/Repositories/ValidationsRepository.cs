using AlxBudgetDesktop.Data.Json;
using AlxBudgetDesktop.Data.Paths;

namespace AlxBudgetDesktop.Data.Repositories;

/// <summary>Persiste validations-&lt;annee&gt;.json, map "AAAA-MM" → date ISO de validation.</summary>
public sealed class ValidationsRepository(DataPathProvider chemins, JsonFileStore store)
{
    private string Chemin(int annee) => chemins.CheminFichier($"validations-{annee}.json");

    public Dictionary<string, string> Charger(int annee)
        => store.LoadOrCreateJson(Chemin(annee), () => new Dictionary<string, string>());

    public Dictionary<string, string> Valider(int annee, string cleMois, DateOnly dateValidation)
    {
        var validations = Charger(annee);
        validations[cleMois] = dateValidation.ToString("yyyy-MM-dd");
        store.SaveJsonAtomic(Chemin(annee), validations);
        return validations;
    }

    public Dictionary<string, string> Devalider(int annee, string cleMois)
    {
        var validations = Charger(annee);
        validations.Remove(cleMois);
        store.SaveJsonAtomic(Chemin(annee), validations);
        return validations;
    }
}
