using System.Text.Json.Serialization;

namespace Fluxo.Domain.Models;

public sealed class GroupePostes
{
    [JsonPropertyName("nom")]
    public required string Nom { get; set; }

    [JsonPropertyName("postes")]
    public required List<string> Postes { get; set; }
}
