namespace Fluxo.Domain.Rapprochement;

/// <summary>
/// Une ligne du relevé bancaire, telle qu'extraite par le parseur CSV.
/// </summary>
public sealed class LigneReleve
{
    /// <summary>Index de la ligne dans le tableau source — sert d'identité stable
    /// pour la réservation pendant le rapprochement (le TS utilise l'identité de
    /// référence de l'objet, non transposable telle quelle en C#).</summary>
    public required int Index { get; init; }

    public required DateOnly DatePrelevement { get; init; }

    /// <summary>Signé : négatif = débit, positif = crédit.</summary>
    public required decimal Montant { get; init; }

    public required string RaisonSociale { get; init; }

    public required string LibelleBrut { get; init; }
}
