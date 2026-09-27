using Fluxo.Data.Json;
using Fluxo.Data.Paths;
using Fluxo.Domain.Models;

namespace Fluxo.Data.Repositories;

/// <summary>
/// Persiste uniquement les calages manuels (soldes-&lt;annee&gt;.json), map "AAAA-MM" →
/// SoldesComptes. Les soldes calculés des 12 mois ne sont jamais stockés ici — ils sont
/// recalculés à la demande par SoldesService à partir de ces calages et des achats.
/// </summary>
public sealed class SoldesRepository(DataPathProvider chemins, JsonFileStore store)
{
    private string Chemin(int annee) => chemins.CheminFichier($"soldes-{annee}.json");

    public Dictionary<string, SoldesComptes> ChargerCalages(int annee)
        => store.LoadOrCreateJson(Chemin(annee), () => new Dictionary<string, SoldesComptes>());

    public bool ExisteFichier(int annee) => File.Exists(Chemin(annee));

    public Dictionary<string, SoldesComptes> DefinirCalage(int annee, string cleMois, SoldesComptes soldes)
    {
        var calages = ChargerCalages(annee);
        calages[cleMois] = soldes;
        store.SaveJsonAtomic(Chemin(annee), calages);
        return calages;
    }

    public Dictionary<string, SoldesComptes> SupprimerCalage(int annee, string cleMois)
    {
        var calages = ChargerCalages(annee);
        calages.Remove(cleMois);
        store.SaveJsonAtomic(Chemin(annee), calages);
        return calages;
    }
}
