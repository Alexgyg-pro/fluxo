namespace Fluxo.Domain;

/// <summary>
/// Noms de groupes/postes reconnus par comparaison de chaîne exacte, qui pilotent
/// des comportements métier particuliers (classification recette/dépense, épargne,
/// remises). Reproduit à l'identique POSTES_SPECIAUX côté alxBudget : si l'utilisateur
/// renomme un de ces groupes/postes via l'écran Admin, le comportement spécial associé
/// est rompu silencieusement — comportement fragile mais volontairement repris tel quel.
/// </summary>
public static class PostesSpeciaux
{
    public const string GroupeRevenus = "Revenus";
    public const string GroupeEpargne = "Épargne";
    public const string GroupeRemises = "Remises";
    public const string EpargneDepot = "Épargne (dépôt)";
    public const string EpargneRetrait = "Épargne (retrait)";
    public const string TicketsRestaurant = "Tickets restaurant";
}
