using System.Text.Json;
using System.Text.Json.Serialization;
using Fluxo.Domain.Models;

namespace Fluxo.Data.Json.Converters;

/// <summary>Sérialise l'enum Compte avec les mêmes littéraux que le TS : "courant" / "ticketRestaurant".</summary>
public sealed class CompteJsonConverter : JsonConverter<Compte>
{
    public override Compte Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => reader.GetString() switch
        {
            "courant" => Compte.Courant,
            "ticketRestaurant" => Compte.TicketRestaurant,
            var autre => throw new JsonException($"Valeur de compte inconnue : '{autre}'."),
        };

    public override void Write(Utf8JsonWriter writer, Compte value, JsonSerializerOptions options)
        => writer.WriteStringValue(value switch
        {
            Compte.Courant => "courant",
            Compte.TicketRestaurant => "ticketRestaurant",
            _ => throw new JsonException($"Valeur de compte inconnue : '{value}'."),
        });
}
