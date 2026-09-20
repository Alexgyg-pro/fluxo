using AlxBudgetDesktop.Domain.Models;

namespace AlxBudgetDesktop.App.ViewModels.Mois;

/// <summary>Habillage d'affichage d'un <see cref="Achat"/> pour la ligne du tableau des
/// opérations du mois — la classification recette/dépense et le formatage restent ici,
/// pas dans le modèle de domaine.</summary>
public sealed class AchatLigneViewModel(Achat achat, bool estRecette, bool estOrpheline = false)
{
    public Achat Achat { get; } = achat;

    public Guid Id => Achat.Id;
    public DateOnly DateAchat => Achat.DateAchat;
    public string Etiquette => Achat.Etiquette;
    public decimal MontantAchat => Achat.MontantAchat;
    public bool EstRecette { get; } = estRecette;

    /// <summary>Dépense saisie sans ligne correspondante au relevé contrôlé
    /// ("Absente du relevé") — calculé par le contrôle de rapprochement éphémère.</summary>
    public bool EstOrpheline { get; } = estOrpheline;

    public string CompteAffiche => Achat.Compte == Compte.TicketRestaurant ? "TR" : "courant";

    public string PostesAffichage
        => string.Join(", ", Achat.Postes.Select(p => $"{p.Key} ({p.Value:0.00} €)"));
}
