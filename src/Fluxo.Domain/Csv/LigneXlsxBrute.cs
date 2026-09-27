namespace Fluxo.Domain.Csv;

/// <summary>
/// Une ligne brute déjà tabulée d'un onglet de relevé xlsx (lue par
/// Fluxo.Data.Xlsx.XlsxReleveFileReader), avant nettoyage par
/// ReleveXlsxParser. Pas de dépendance à une bibliothèque xlsx ici : Domain
/// reste pur.
/// </summary>
public sealed class LigneXlsxBrute
{
    public required string? Date { get; init; }
    public required string? Operation { get; init; }
    public required string? Detail { get; init; }
    public required decimal? Debit { get; init; }
    public required decimal? Credit { get; init; }
}
