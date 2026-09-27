using System.Text.Json.Serialization;

namespace Fluxo.Domain.Models;

public sealed class SoldesComptes
{
    [JsonPropertyName("courant")]
    public decimal Courant { get; init; }

    [JsonPropertyName("ticketRestaurant")]
    public decimal TicketRestaurant { get; init; }

    [JsonPropertyName("epargne")]
    public decimal Epargne { get; init; }

    public static SoldesComptes Zero => new() { Courant = 0, TicketRestaurant = 0, Epargne = 0 };

    public static SoldesComptes operator +(SoldesComptes a, SoldesComptes b) => new()
    {
        Courant = Arrondi.Arrondi2(a.Courant + b.Courant),
        TicketRestaurant = Arrondi.Arrondi2(a.TicketRestaurant + b.TicketRestaurant),
        Epargne = Arrondi.Arrondi2(a.Epargne + b.Epargne),
    };
}
