using System.Text.Json;
using System.Text.Json.Serialization;

namespace AlxBudgetDesktop.Data.Json.Converters;

/// <summary>
/// Le format alxBudget encode une date de prélèvement absente par une chaîne vide
/// "" (jamais null, jamais une clé omise) — reproduit ici pour DateOnly?.
/// </summary>
public sealed class NullableDateOnlyEmptyStringConverter : JsonConverter<DateOnly?>
{
    private const string Format = "yyyy-MM-dd";

    public override DateOnly? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var s = reader.GetString();
        return string.IsNullOrEmpty(s) ? null : DateOnly.ParseExact(s, Format);
    }

    public override void Write(Utf8JsonWriter writer, DateOnly? value, JsonSerializerOptions options)
        => writer.WriteStringValue(value?.ToString(Format) ?? "");
}
