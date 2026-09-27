using Fluxo.Domain.Budget;
using Fluxo.Domain.Models;
using Fluxo.Domain.Services;
using Xunit;

namespace Fluxo.Domain.Tests;

public class BudgetServiceTests
{
    private static Achat Depense(string poste, decimal montant, DateOnly dateAchat)
        => new()
        {
            Id = Guid.NewGuid(),
            DateAchat = dateAchat,
            Etiquette = "Test",
            MontantAchat = montant,
            Compte = Compte.Courant,
            Postes = new Dictionary<string, decimal> { [poste] = montant },
        };

    private static readonly List<GroupePostes> Groupes =
    [
        new() { Nom = "Revenus", Postes = ["Salaire"] },
        new() { Nom = "Alimentation", Postes = ["Courses"] },
    ];

    [Fact]
    public void Mois_courant_est_toujours_exclu_de_la_moyenne_meme_saisi()
    {
        // Jan-avr non renseignés, mai/juin/juil à 100€, août (mois courant) à 50€
        // → projection sept-déc = moyenne de (100+100+100)/3 = 100€, pas (100+100+100+50)/12.
        var aujourdhui = new DateOnly(2025, 8, 15);
        var achats = new List<Achat>
        {
            Depense("Courses", 100m, new DateOnly(2025, 5, 1)),
            Depense("Courses", 100m, new DateOnly(2025, 6, 1)),
            Depense("Courses", 100m, new DateOnly(2025, 7, 1)),
            Depense("Courses", 50m, new DateOnly(2025, 8, 1)),
        };

        var budget = BudgetService.CalculerBudget(2025, achats, achats, Groupes, aujourdhui);

        var ligneCourses = budget.Groupes.Single(g => g.Nom == "Alimentation").Lignes.Single(l => l.Poste == "Courses");
        Assert.Equal(100m, ligneCourses.Mensuel[8]);  // septembre
        Assert.Equal(100m, ligneCourses.Mensuel[11]); // décembre
        Assert.Equal(50m, ligneCourses.Mensuel[7]);   // août (mois courant) reste sa valeur réelle
    }

    [Fact]
    public void Aucun_mois_clos_renseigne_laisse_les_mois_projetes_a_zero()
    {
        var aujourdhui = new DateOnly(2025, 3, 10);
        var achats = new List<Achat>();

        var budget = BudgetService.CalculerBudget(2025, achats, achats, Groupes, aujourdhui);

        var ligneCourses = budget.Groupes.Single(g => g.Nom == "Alimentation").Lignes.Single(l => l.Poste == "Courses");
        Assert.All(ligneCourses.Mensuel, v => Assert.Equal(0m, v));
    }

    [Fact]
    public void Mois_a_zero_euro_compte_comme_renseigne_dans_le_denominateur_de_la_moyenne()
    {
        // Janvier à 0€ (renseigné) et février à 300€ (renseigné), tous deux clos avant avril.
        // Si le mois à 0€ n'était PAS compté comme renseigné, la moyenne serait 300/1=300€ ;
        // en le comptant, elle doit être (0+300)/2=150€.
        var aujourdhui = new DateOnly(2025, 4, 1);
        var achats = new List<Achat>
        {
            Depense("Courses", 0m, new DateOnly(2025, 1, 15)),
            Depense("Courses", 300m, new DateOnly(2025, 2, 15)),
        };

        var budget = BudgetService.CalculerBudget(2025, achats, achats, Groupes, aujourdhui);

        var ligneCourses = budget.Groupes.Single(g => g.Nom == "Alimentation").Lignes.Single(l => l.Poste == "Courses");
        Assert.Equal(150m, ligneCourses.Mensuel[4]); // mai, mois projeté
    }

    [Fact]
    public void Solde_mensuel_ajoute_les_revenus_et_soustrait_tous_les_autres_groupes()
    {
        var aujourdhui = new DateOnly(2025, 6, 15);
        var achats = new List<Achat>
        {
            new()
            {
                Id = Guid.NewGuid(), DateAchat = new DateOnly(2025, 6, 1), Etiquette = "Salaire",
                MontantAchat = 2000m, Compte = Compte.Courant, Postes = new() { ["Salaire"] = 2000m },
            },
            Depense("Courses", 300m, new DateOnly(2025, 6, 5)),
        };

        var budget = BudgetService.CalculerBudget(2025, achats, achats, Groupes, aujourdhui);

        Assert.Equal(1700m, budget.SoldeMensuel[5]); // juin, index 5
    }

    [Fact]
    public void Annee_passee_na_plus_aucun_mois_futur_a_projeter()
    {
        var aujourdhui = new DateOnly(2025, 3, 1);
        var achats = new List<Achat>
        {
            Depense("Courses", 100m, new DateOnly(2024, 1, 1)),
        };

        var typesMois = BudgetService.CalculerTypesMois(2024, aujourdhui);
        Assert.All(typesMois, t => Assert.Equal(TypeMois.Reel, t));
    }
}
