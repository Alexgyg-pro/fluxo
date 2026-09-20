using AlxBudgetDesktop.Data.Repositories;
using AlxBudgetDesktop.Domain.Models;
using AlxBudgetDesktop.Domain.Services;

namespace AlxBudgetDesktop.Data.Services;

/// <summary>
/// Calcule les soldes d'une année en chaînant l'ouverture de janvier sur la clôture de
/// décembre de l'année précédente, quand elle existe — port de
/// routes/soldes.ts::ouvertureJanvierDe côté alxBudget. SoldesService (Domain) reste pur
/// et ne connaît pas les années précédentes ; ce service, côté Data, orchestre les
/// repositories (achats/postes/soldes) pour la remontée récursive tant qu'une année
/// précédente a des données, avec un plancher pour éviter toute récursion infinie.
/// </summary>
public sealed class SoldesAnneeService(
    AchatsRepository achatsRepository,
    PostesRepository postesRepository,
    SoldesRepository soldesRepository)
{
    private const int AnneePlancher = 2000;

    public SoldesAnnee CalculerSoldesAnnee(int annee)
    {
        var groupes = postesRepository.Charger();
        var achatsAnnee = achatsRepository.ListerAchats(annee);
        var calages = soldesRepository.ChargerCalages(annee);
        var ouvertureJanvier = OuvertureJanvierDe(annee);

        return SoldesService.CalculerSoldesAnnee(annee, achatsAnnee, calages, groupes, ouvertureJanvier);
    }

    private SoldesComptes OuvertureJanvierDe(int annee)
    {
        var anneePrecedente = annee - 1;
        if (anneePrecedente < AnneePlancher || !AnneeADesDonnees(anneePrecedente))
        {
            return SoldesComptes.Zero;
        }

        // Récursif : peut remonter toute une chaîne d'années précédentes tant qu'elles
        // ont des données. Non mémoïsé, comme côté alxBudget — sans conséquence à
        // l'échelle d'un usage personnel (quelques années, quelques centaines d'achats).
        var soldesAnneePrecedente = CalculerSoldesAnnee(anneePrecedente);
        return soldesAnneePrecedente.Mois[11].Disponible;
    }

    private bool AnneeADesDonnees(int annee) => achatsRepository.ExisteFichier(annee) || soldesRepository.ExisteFichier(annee);
}
