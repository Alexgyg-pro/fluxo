using Fluxo.Data.Json;
using Fluxo.Data.Paths;

namespace Fluxo.Data.Repositories;

public sealed class BeneficiairesRepository(DataPathProvider chemins, JsonFileStore store)
{
    private string Chemin => chemins.CheminFichier("beneficiaires.json");

    public Dictionary<string, List<string>> Charger()
        => store.LoadOrCreateJson(Chemin, () => new Dictionary<string, List<string>>());

    public void Enregistrer(Dictionary<string, List<string>> beneficiaires) => store.SaveJsonAtomic(Chemin, beneficiaires);

    /// <summary>Enrichit le registre avec une association raison sociale → postes (utilisé par l'import CSV).</summary>
    public void EnregistrerAssociation(string cleRaisonSociale, IReadOnlyList<string> postes)
    {
        if (string.IsNullOrWhiteSpace(cleRaisonSociale) || postes.Count == 0)
        {
            return;
        }

        var beneficiaires = Charger();
        beneficiaires[cleRaisonSociale] = postes.ToList();
        Enregistrer(beneficiaires);
    }
}
