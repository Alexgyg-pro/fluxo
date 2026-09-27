using Fluxo.Data.Json;
using Fluxo.Data.Paths;
using Fluxo.Domain.Models;

namespace Fluxo.Data.Repositories;

public sealed class ConfigRepository(DataPathProvider chemins, JsonFileStore store)
{
    private string Chemin => chemins.CheminFichier("config.json");

    public Config Charger()
        => store.LoadOrCreateJson(Chemin, () => new Config { AnneeCourante = DateTime.Now.Year });

    public void Enregistrer(Config config) => store.SaveJsonAtomic(Chemin, config);
}
