using AlxBudgetDesktop.Domain.Models;
using AlxBudgetDesktop.Domain.Services;
using Xunit;

namespace AlxBudgetDesktop.Domain.Tests;

public class AchatValidatorTests
{
    private static AchatInput Input(decimal montant, Dictionary<string, decimal> postes, string etiquette = "Test")
        => new()
        {
            DateAchat = new DateOnly(2025, 6, 1),
            Etiquette = etiquette,
            MontantAchat = montant,
            Compte = Compte.Courant,
            Postes = postes,
        };

    [Fact]
    public void Achat_valide_ne_leve_aucune_erreur()
    {
        var erreurs = AchatValidator.Valider(Input(12m, new() { ["Alimentation"] = 12m }));
        Assert.Empty(erreurs);
    }

    [Fact]
    public void Somme_des_postes_differente_du_montant_est_rejetee()
    {
        var erreurs = AchatValidator.Valider(Input(12m, new() { ["Alimentation"] = 10m }));
        Assert.Contains(erreurs, e => e.Contains("somme des postes", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Montant_negatif_ou_nul_est_rejete()
    {
        var erreurs = AchatValidator.Valider(Input(0m, new() { ["Alimentation"] = 0m }));
        Assert.Contains(erreurs, e => e.Contains("strictement positif", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Etiquette_vide_est_rejetee()
    {
        var erreurs = AchatValidator.Valider(Input(12m, new() { ["Alimentation"] = 12m }, etiquette: "   "));
        Assert.Contains(erreurs, e => e.Contains("étiquette", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Remise_negative_est_autorisee_si_la_somme_algebrique_egale_le_montant()
    {
        // Ticket 15€ de produits payé 12€ grâce à 3€ de cagnotte : la remise se stocke
        // en négatif, la somme des postes doit rester égale au montant réellement débité.
        var postes = new Dictionary<string, decimal>
        {
            ["Alimentation"] = 15m,
            ["Cagnottes fidélité"] = -3m,
        };

        var erreurs = AchatValidator.Valider(Input(12m, postes));
        Assert.Empty(erreurs);
    }

    [Fact]
    public void Aucun_poste_est_rejete()
    {
        var erreurs = AchatValidator.Valider(Input(12m, new Dictionary<string, decimal>()));
        Assert.Contains(erreurs, e => e.Contains("poste", StringComparison.OrdinalIgnoreCase));
    }
}
