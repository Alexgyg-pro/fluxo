using AlxBudgetDesktop.Data.Json;
using AlxBudgetDesktop.Data.Paths;
using AlxBudgetDesktop.Domain.Models;

namespace AlxBudgetDesktop.Data.Repositories;

public sealed class ConfigRepository(DataPathProvider chemins, JsonFileStore store)
{
    private string Chemin => chemins.CheminFichier("config.json");

    public Config Charger()
        => store.LoadOrCreateJson(Chemin, () => new Config { AnneeCourante = DateTime.Now.Year });

    public void Enregistrer(Config config) => store.SaveJsonAtomic(Chemin, config);
}
