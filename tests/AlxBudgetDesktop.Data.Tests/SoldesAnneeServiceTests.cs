using AlxBudgetDesktop.Data.Json;
using AlxBudgetDesktop.Data.Paths;
using AlxBudgetDesktop.Data.Repositories;
using AlxBudgetDesktop.Data.Services;
using AlxBudgetDesktop.Domain.Models;
using Xunit;

namespace AlxBudgetDesktop.Data.Tests;

/// <summary>Vérifie le chaînage du solde d'ouverture entre années (US-002), sur un
/// dossier de données temporaire et isolé, jamais le vrai dossier de l'utilisateur.</summary>
public sealed class SoldesAnneeServiceTests : IDisposable
{
    private readonly string _dossierTemp;
    private readonly AchatsRepository _achatsRepository;
    private readonly SoldesRepository _soldesRepository;
    private readonly SoldesAnneeService _service;

    public SoldesAnneeServiceTests()
    {
        _dossierTemp = Path.Combine(Path.GetTempPath(), "alxbudgetdesktop-tests-" + Guid.NewGuid());
        var chemins = new DataPathProvider(_dossierTemp);
        var store = new JsonFileStore();

        _achatsRepository = new AchatsRepository(chemins, store);
        var postesRepository = new PostesRepository(chemins, store);
        _soldesRepository = new SoldesRepository(chemins, store);
        _service = new SoldesAnneeService(_achatsRepository, postesRepository, _soldesRepository);
    }

    public void Dispose()
    {
        if (Directory.Exists(_dossierTemp))
        {
            Directory.Delete(_dossierTemp, recursive: true);
        }
    }

    private static AchatInput Salaire(decimal montant, DateOnly date)
        => new()
        {
            DateAchat = date,
            Etiquette = "Salaire",
            MontantAchat = montant,
            Compte = Compte.Courant,
            Postes = new Dictionary<string, decimal> { ["Salaire"] = montant },
        };

    private static AchatInput Depense(decimal montant, DateOnly date)
        => new()
        {
            DateAchat = date,
            Etiquette = "Courses",
            MontantAchat = montant,
            Compte = Compte.Courant,
            Postes = new Dictionary<string, decimal> { ["Alimentation"] = montant },
        };

    [Fact]
    public void Sans_annee_precedente_ouverture_de_janvier_est_zero()
    {
        _achatsRepository.Ajouter(Salaire(1000m, new DateOnly(2026, 1, 5)));

        var soldes = _service.CalculerSoldesAnnee(2026);

        Assert.Equal(0m, soldes.Mois[0].Ouverture.Courant);
        Assert.Equal(1000m, soldes.Mois[0].Disponible.Courant);
    }

    [Fact]
    public void Chaine_l_ouverture_de_janvier_sur_la_cloture_de_decembre_precedent()
    {
        _achatsRepository.Ajouter(Salaire(2000m, new DateOnly(2025, 12, 1)));
        _achatsRepository.Ajouter(Depense(300m, new DateOnly(2026, 1, 10)));

        var soldes2026 = _service.CalculerSoldesAnnee(2026);

        Assert.Equal(2000m, soldes2026.Mois[0].Ouverture.Courant);
        Assert.Equal(1700m, soldes2026.Mois[0].Disponible.Courant);
    }

    [Fact]
    public void Chaine_recursivement_sur_plusieurs_annees_successives()
    {
        _achatsRepository.Ajouter(Salaire(500m, new DateOnly(2024, 6, 1)));
        _achatsRepository.Ajouter(Salaire(100m, new DateOnly(2025, 3, 1)));

        var soldes2026 = _service.CalculerSoldesAnnee(2026);

        // 2024 clôture à 500 -> report vers 2025 -> 2025 clôture à 600 -> report vers 2026.
        Assert.Equal(600m, soldes2026.Mois[0].Ouverture.Courant);
    }

    [Fact]
    public void Un_calage_manuel_en_decembre_precedent_se_propage_aussi()
    {
        _soldesRepository.DefinirCalage(2025, "2025-12", new SoldesComptes { Courant = 5000m, TicketRestaurant = 0m, Epargne = 0m });

        var soldes2026 = _service.CalculerSoldesAnnee(2026);

        Assert.Equal(5000m, soldes2026.Mois[0].Ouverture.Courant);
    }

    [Fact]
    public void Aucune_donnee_avant_2000_ouverture_reste_zero_meme_avec_un_fichier_proche_du_plancher()
    {
        // Le plancher (2000) coupe la remontée récursive avant d'atteindre une année
        // antérieure — pas de comportement observable différent ici sans données
        // en l'an 2000, mais on vérifie au moins l'absence de récursion infinie /
        // d'exception sur une petite année.
        var soldes = _service.CalculerSoldesAnnee(2001);

        Assert.Equal(0m, soldes.Mois[0].Ouverture.Courant);
    }
}
