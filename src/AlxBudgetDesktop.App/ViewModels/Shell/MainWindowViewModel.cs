using System.Collections.ObjectModel;
using AlxBudgetDesktop.App.Mvvm;
using AlxBudgetDesktop.App.ViewModels.Admin;
using AlxBudgetDesktop.App.ViewModels.Budget;
using AlxBudgetDesktop.App.ViewModels.Import;
using AlxBudgetDesktop.App.ViewModels.Mois;
using AlxBudgetDesktop.Data.Repositories;

namespace AlxBudgetDesktop.App.ViewModels.Shell;

/// <summary>
/// ViewModel racine : construit les 12 onglets mois + Budget/Import/Admin pour l'année
/// courante et pilote l'onglet sélectionné. Pas de vrai routeur — le changement d'onglet
/// est une simple affectation de <see cref="OngletSelectionne"/>, rendue par un
/// DataTemplate keyé par type de ViewModel côté XAML (App.xaml).
/// </summary>
public sealed class MainWindowViewModel : ObservableObject
{
    public ObservableCollection<TabViewModelBase> Onglets { get; } = [];

    private TabViewModelBase _ongletSelectionne;
    public TabViewModelBase OngletSelectionne
    {
        get => _ongletSelectionne;
        set => SetProperty(ref _ongletSelectionne, value);
    }

    public MainWindowViewModel(ConfigRepository configRepository, ValidationsRepository validationsRepository)
    {
        var annee = configRepository.Charger().AnneeCourante;
        var validations = validationsRepository.Charger(annee);

        for (var mois = 0; mois < 12; mois++)
        {
            var cleMois = Domain.Services.SoldesService.CleMois(annee, mois);
            Onglets.Add(new MoisViewModel(mois, annee, estValide: validations.ContainsKey(cleMois)));
        }

        Onglets.Add(new BudgetViewModel());
        Onglets.Add(new ImportViewModel());
        Onglets.Add(new AdminViewModel());

        _ongletSelectionne = Onglets[0];
    }
}
