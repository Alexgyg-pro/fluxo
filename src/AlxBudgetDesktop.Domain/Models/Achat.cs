using System.Text.Json.Serialization;

namespace AlxBudgetDesktop.Domain.Models;

/// <summary>
/// Une dépense ou une recette. Le champ qui détermine s'il s'agit d'une recette
/// n'existe pas explicitement : c'est la présence d'au moins un poste du groupe
/// "Revenus" (<see cref="PostesSpeciaux.GroupeRevenus"/>) dans <see cref="Postes"/>
/// qui classe l'opération. L'ordre de déclaration des propriétés est important :
/// System.Text.Json sérialise dans cet ordre, et il doit rester identique à celui
/// des fichiers achats-&lt;année&gt;.jsonl produits par alxBudget.
/// </summary>
public sealed class Achat
{
    [JsonPropertyName("id")]
    public required Guid Id { get; init; }

    [JsonPropertyName("rapproche")]
    public bool Rapproche { get; init; }

    [JsonPropertyName("dateAchat")]
    public required DateOnly DateAchat { get; init; }

    [JsonPropertyName("etiquette")]
    public required string Etiquette { get; init; }

    [JsonPropertyName("datePrelevement")]
    public DateOnly? DatePrelevement { get; init; }

    [JsonPropertyName("raisonSociale")]
    public string RaisonSociale { get; init; } = "";

    [JsonPropertyName("montantAchat")]
    public required decimal MontantAchat { get; init; }

    [JsonPropertyName("compte")]
    public required Compte Compte { get; init; }

    [JsonPropertyName("postes")]
    public required Dictionary<string, decimal> Postes { get; init; }

    [JsonPropertyName("libelleReleve")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? LibelleReleve { get; init; }
}

/// <summary>
/// Payload de création/modification d'un achat : mêmes champs qu'<see cref="Achat"/>
/// sans <see cref="Achat.Id"/> ni <see cref="Achat.Rapproche"/> (calculés par le repository).
/// </summary>
public sealed class AchatInput
{
    public required DateOnly DateAchat { get; init; }
    public required string Etiquette { get; init; }
    public DateOnly? DatePrelevement { get; init; }
    public string RaisonSociale { get; init; } = "";
    public required decimal MontantAchat { get; init; }
    public required Compte Compte { get; init; }
    public required Dictionary<string, decimal> Postes { get; init; }
    public string? LibelleReleve { get; init; }
}
