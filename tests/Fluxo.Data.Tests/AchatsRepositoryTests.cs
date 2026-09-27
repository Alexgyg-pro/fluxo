using Fluxo.Data.Json;
using Fluxo.Data.Paths;
using Fluxo.Data.Repositories;
using Fluxo.Domain.Models;
using Fluxo.Domain.Rapprochement;
using Xunit;

namespace Fluxo.Data.Tests;

/// <summary>Vérifie la persistance du rapprochement (US-008), sur un dossier de données
/// temporaire et isolé, jamais le vrai dossier de l'utilisateur.</summary>
public sealed class AchatsRepositoryTests : IDisposable
{
    private readonly string _dossierTemp;
    private readonly AchatsRepository _achatsRepository;

    public AchatsRepositoryTests()
    {
        _dossierTemp = Path.Combine(Path.GetTempPath(), "fluxo-tests-" + Guid.NewGuid());
        var chemins = new DataPathProvider(_dossierTemp);
        var store = new JsonFileStore();
        _achatsRepository = new AchatsRepository(chemins, store);
    }

    public void Dispose()
    {
        if (Directory.Exists(_dossierTemp))
        {
            Directory.Delete(_dossierTemp, recursive: true);
        }
    }

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
    public void AppliquerRapprochements_marque_rapproche_et_complete_les_infos_bancaires()
    {
        var achat = _achatsRepository.Ajouter(Depense(30.01m, new DateOnly(2026, 7, 31)));
        var ligne = new LigneReleve
        {
            Index = 0,
            DatePrelevement = new DateOnly(2026, 8, 3),
            Montant = -30.01m,
            RaisonSociale = "SUPERETTE DEMO",
            LibelleBrut = "PAIEMENT PSC 3107 FICTIVILLE SUPERETTE DEMO CARTE 5299",
        };

        _achatsRepository.AppliquerRapprochements(2026, [new RapprochementMatch { AchatId = achat.Id, Ligne = ligne }]);

        var relu = _achatsRepository.ListerAchats(2026).Single(a => a.Id == achat.Id);
        Assert.True(relu.Rapproche);
        Assert.Equal(new DateOnly(2026, 8, 3), relu.DatePrelevement);
        Assert.Equal("SUPERETTE DEMO", relu.RaisonSociale);
        Assert.Equal(ligne.LibelleBrut, relu.LibelleReleve);
    }

    [Fact]
    public void AppliquerRapprochements_ne_touche_pas_les_achats_absents_des_matches()
    {
        var achatVise = _achatsRepository.Ajouter(Depense(10m, new DateOnly(2026, 7, 1)));
        var achatIgnore = _achatsRepository.Ajouter(Depense(20m, new DateOnly(2026, 7, 2)));
        var ligne = new LigneReleve { Index = 0, DatePrelevement = new DateOnly(2026, 7, 1), Montant = -10m, RaisonSociale = "X", LibelleBrut = "X" };

        _achatsRepository.AppliquerRapprochements(2026, [new RapprochementMatch { AchatId = achatVise.Id, Ligne = ligne }]);

        var achats = _achatsRepository.ListerAchats(2026);
        Assert.True(achats.Single(a => a.Id == achatVise.Id).Rapproche);
        Assert.False(achats.Single(a => a.Id == achatIgnore.Id).Rapproche);
    }

    [Fact]
    public void ReinitialiserRapprochement_remet_rapproche_a_false_sans_perdre_l_achat()
    {
        var achat = _achatsRepository.Ajouter(Depense(30.01m, new DateOnly(2026, 7, 31)));
        var ligne = new LigneReleve { Index = 0, DatePrelevement = new DateOnly(2026, 8, 3), Montant = -30.01m, RaisonSociale = "SUPERETTE DEMO", LibelleBrut = "..." };
        _achatsRepository.AppliquerRapprochements(2026, [new RapprochementMatch { AchatId = achat.Id, Ligne = ligne }]);

        _achatsRepository.ReinitialiserRapprochement(2026, [achat.Id]);

        var relu = _achatsRepository.ListerAchats(2026).Single(a => a.Id == achat.Id);
        Assert.False(relu.Rapproche);
        Assert.Equal(30.01m, relu.MontantAchat);
        Assert.Equal("Courses", relu.Etiquette);
    }
}
