using AlxBudgetDesktop.Domain.Models;
using AlxBudgetDesktop.Domain.Services;
using Xunit;

namespace AlxBudgetDesktop.Domain.Tests;

public class SoldesServiceTests
{
    private static readonly List<GroupePostes> Groupes =
    [
        new() { Nom = "Revenus", Postes = ["Salaire", "Tickets restaurant", "Épargne (retrait)"] },
        new() { Nom = "Épargne", Postes = ["Épargne (dépôt)"] },
        new() { Nom = "Alimentation", Postes = ["Courses"] },
    ];

    private static Achat Depense(decimal montant, string poste, DateOnly date, Compte compte = Compte.Courant)
        => new()
        {
            Id = Guid.NewGuid(), DateAchat = date, Etiquette = "T", MontantAchat = montant,
            Compte = compte, Postes = new Dictionary<string, decimal> { [poste] = montant },
        };

    [Fact]
    public void Recette_sur_compte_courant_augmente_le_solde_courant()
    {
        var recettes = SoldesService.PostesRecette(Groupes);
        var achats = new List<Achat> { Depense(2000m, "Salaire", new DateOnly(2025, 6, 1)) };

        var mouvements = SoldesService.MouvementsMois(achats, recettes);

        Assert.Equal(2000m, mouvements.Courant);
    }

    [Fact]
    public void Recharge_tickets_restaurant_credite_le_tr_et_est_deduite_du_courant_meme_en_recette()
    {
        // Un salaire de 2000€ dont 100€ sont convertis en TR : le courant ne reçoit que 1900€,
        // le TR reçoit 100€. La part TR ne doit jamais "rester" sur le courant en plus.
        var recettes = SoldesService.PostesRecette(Groupes);
        var achat = new Achat
        {
            Id = Guid.NewGuid(), DateAchat = new DateOnly(2025, 6, 1), Etiquette = "Salaire",
            MontantAchat = 2000m, Compte = Compte.Courant,
            Postes = new Dictionary<string, decimal> { ["Salaire"] = 1900m, ["Tickets restaurant"] = 100m },
        };

        var mouvements = SoldesService.MouvementsMois([achat], recettes);

        Assert.Equal(1900m, mouvements.Courant);
        Assert.Equal(100m, mouvements.TicketRestaurant);
    }

    [Fact]
    public void Depense_sur_compte_ticket_restaurant_deduit_uniquement_le_solde_tr()
    {
        var recettes = SoldesService.PostesRecette(Groupes);
        var achats = new List<Achat> { Depense(15m, "Courses", new DateOnly(2025, 6, 1), Compte.TicketRestaurant) };

        var mouvements = SoldesService.MouvementsMois(achats, recettes);

        Assert.Equal(-15m, mouvements.TicketRestaurant);
        Assert.Equal(0m, mouvements.Courant);
    }

    [Fact]
    public void Epargne_depot_et_retrait_independants_du_compte()
    {
        var recettes = SoldesService.PostesRecette(Groupes);
        var achats = new List<Achat>
        {
            Depense(50m, "Épargne (dépôt)", new DateOnly(2025, 6, 1)),
            Depense(20m, "Épargne (retrait)", new DateOnly(2025, 6, 2)),
        };

        var mouvements = SoldesService.MouvementsMois(achats, recettes);

        Assert.Equal(30m, mouvements.Epargne);
    }

    [Fact]
    public void Report_automatique_chaine_le_disponible_du_mois_precedent()
    {
        var achats = new List<Achat>
        {
            Depense(1000m, "Salaire", new DateOnly(2025, 1, 1)),
            Depense(200m, "Courses", new DateOnly(2025, 1, 5)),
            Depense(500m, "Salaire", new DateOnly(2025, 2, 1)),
        };

        var soldes = SoldesService.CalculerSoldesAnnee(2025, achats, new Dictionary<string, SoldesComptes>(), Groupes);

        Assert.Equal(800m, soldes.Mois[0].Disponible.Courant); // janvier : 0 + 1000 - 200
        Assert.Equal(800m, soldes.Mois[1].Report.Courant);     // février : report = disponible janvier
        Assert.Equal(1300m, soldes.Mois[1].Disponible.Courant); // février : 800 + 500
    }

    [Fact]
    public void Calage_manuel_ecrase_le_report_et_se_propage_en_cascade()
    {
        var achats = new List<Achat>
        {
            Depense(500m, "Salaire", new DateOnly(2025, 3, 1)),
        };

        var calages = new Dictionary<string, SoldesComptes>
        {
            ["2025-02"] = new SoldesComptes { Courant = 5000m, TicketRestaurant = 0m, Epargne = 0m },
        };

        var soldes = SoldesService.CalculerSoldesAnnee(2025, achats, calages, Groupes);

        Assert.True(soldes.Mois[1].Cale); // février calé
        Assert.Equal(5000m, soldes.Mois[1].Ouverture.Courant);
        Assert.Equal(5000m, soldes.Mois[1].Disponible.Courant); // aucun mouvement en février

        // Mars n'est pas calé, mais hérite en cascade du disponible de février calé.
        Assert.False(soldes.Mois[2].Cale);
        Assert.Equal(5000m, soldes.Mois[2].Report.Courant);
        Assert.Equal(5500m, soldes.Mois[2].Disponible.Courant);
    }
}
