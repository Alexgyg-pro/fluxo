using System.Text.Json.Serialization;

namespace Fluxo.Domain.Models;

public sealed class Config
{
    [JsonPropertyName("anneeCourante")]
    public required int AnneeCourante { get; set; }
}
