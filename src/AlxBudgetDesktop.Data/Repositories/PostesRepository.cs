using AlxBudgetDesktop.Data.Json;
using AlxBudgetDesktop.Data.Paths;
using AlxBudgetDesktop.Domain;
using AlxBudgetDesktop.Domain.Models;

namespace AlxBudgetDesktop.Data.Repositories;

public sealed class PostesRepository(DataPathProvider chemins, JsonFileStore store)
{
    private string Chemin => chemins.CheminFichier("postes.json");

    /// <summary>Seed initial — inspiré de POSTES_DEFAUT côté alxBudget (server/src/store/defaults.ts),
    /// à l'ordre des groupes près (Revenus en tête, demande PO) et généricisé pour rester neutre
    /// dans un projet destiné à être partagé publiquement (ex. "Huissier" retiré).</summary>
    public static List<GroupePostes> PostesDefaut() =>
    [
        new() { Nom = PostesSpeciaux.GroupeRevenus, Postes = ["Salaire", PostesSpeciaux.TicketsRestaurant, PostesSpeciaux.EpargneRetrait, "Divers"] },
        new() { Nom = "Dépenses incontournables", Postes = ["Loyer + charges", "Électricité", "Internet", "Téléphone portable"] },
        new()
        {
            Nom = "Dépenses domestiques",
            Postes =
            [
                "Alimentation", "Restauration", "Alcool", "Consommables",
                "Mobilier et décoration", "Vêtements et accessoires", "Sorties et dépenses culturelles",
            ],
        },
        new() { Nom = "Transports", Postes = ["Abonnement transports"] },
        new() { Nom = PostesSpeciaux.GroupeEpargne, Postes = [PostesSpeciaux.EpargneDepot] },
        new() { Nom = "Dépenses extraordinaires", Postes = ["Imprévus"] },
        new() { Nom = "Dépenses diverses", Postes = ["Frais bancaires"] },
        new() { Nom = PostesSpeciaux.GroupeRemises, Postes = ["Cagnottes fidélité"] },
    ];

    public List<GroupePostes> Charger()
    {
        var groupes = store.LoadOrCreateJson(Chemin, PostesDefaut);
        return AssurerGroupeRemises(groupes);
    }

    public void Enregistrer(List<GroupePostes> groupes) => store.SaveJsonAtomic(Chemin, groupes);

    /// <summary>Migration douce : ajoute le groupe Remises s'il manque (jeu de données créé avant son introduction).</summary>
    private List<GroupePostes> AssurerGroupeRemises(List<GroupePostes> groupes)
    {
        if (groupes.Any(g => g.Nom == PostesSpeciaux.GroupeRemises))
        {
            return groupes;
        }

        groupes.Add(new GroupePostes { Nom = PostesSpeciaux.GroupeRemises, Postes = ["Cagnottes fidélité"] });
        Enregistrer(groupes);
        return groupes;
    }
}
