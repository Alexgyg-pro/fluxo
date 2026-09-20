using System.Windows;
using AlxBudgetDesktop.App.ViewModels.Shell;
using AlxBudgetDesktop.App.Views.Shell;
using AlxBudgetDesktop.Data.Csv;
using AlxBudgetDesktop.Data.Json;
using AlxBudgetDesktop.Data.Paths;
using AlxBudgetDesktop.Data.Repositories;

namespace AlxBudgetDesktop.App;

/// <summary>
/// Composition root : pas de conteneur IoC, les dépendances sont construites et
/// injectées explicitement par constructeur — volontairement simple pour un projet
/// solo à but pédagogique de cette taille (4 projets, une poignée d'écrans).
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        CicCsvFileReader.EnregistrerEncodages();

        var chemins = new DataPathProvider();
        var store = new JsonFileStore();

        var configRepository = new ConfigRepository(chemins, store);
        var validationsRepository = new ValidationsRepository(chemins, store);

        var mainWindowViewModel = new MainWindowViewModel(configRepository, validationsRepository);

        var mainWindow = new MainWindow { DataContext = mainWindowViewModel };
        mainWindow.Show();
    }
}
