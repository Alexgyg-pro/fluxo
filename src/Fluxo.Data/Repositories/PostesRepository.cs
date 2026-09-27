using Fluxo.Data.Json;
using Fluxo.Data.Paths;
using Fluxo.Domain;
using Fluxo.Domain.Models;

namespace Fluxo.Data.Repositories;

public sealed class PostesRepository(DataPathProvider chemins, JsonFileStore store)
{
    private string Chemin => chemins.CheminFichier("postes.json");

    /// <summary>Seed initial, entièrement passe-partout — validé par le PO avant codage en dur
    /// (aucune référence personnelle, projet destiné à être partagé publiquement). Les noms de
    /// groupes/postes marqués via PostesSpeciaux restent figés par le code métier.</summary>
    public static List<GroupePostes> PostesDefaut() =>
    [
        new() { Nom = PostesSpeciaux.GroupeRevenus, Postes = ["Salaire", PostesSpeciaux.TicketsRestaurant, PostesSpeciaux.EpargneRetrait, "Autres revenus"] },
        new() { Nom = "Logement", Postes = ["Loyer ou crédit immobilier", "Charges et copropriété", "Électricité et gaz", "Eau", "Internet et téléphone", "Assurance habitation"] },
        new() { Nom = "Vie quotidienne", Postes = ["Alimentation", "Restauration", "Vêtements et accessoires", "Santé", "Loisirs et culture"] },
        new() { Nom = "Transports", Postes = ["Carburant et transports en commun", "Assurance et entretien véhicule"] },
        new() { Nom = PostesSpeciaux.GroupeEpargne, Postes = [PostesSpeciaux.EpargneDepot] },
        new() { Nom = "Imprévus", Postes = ["Imprévus"] },
        new() { Nom = "Frais divers", Postes = ["Frais bancaires", "Abonnements"] },
        new() { Nom = PostesSpeciaux.GroupeRemises, Postes = ["Remises et avoirs"] },
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
