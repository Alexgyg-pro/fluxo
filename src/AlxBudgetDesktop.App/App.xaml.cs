using System.Windows;
using AlxBudgetDesktop.App.Mvvm;
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

        try
        {
            CicCsvFileReader.EnregistrerEncodages();

            var chemins = new DataPathProvider();
            var store = new JsonFileStore();

            var configRepository = new ConfigRepository(chemins, store);
            var validationsRepository = new ValidationsRepository(chemins, store);
            var postesRepository = new PostesRepository(chemins, store);
            var achatsRepository = new AchatsRepository(chemins, store);
            var soldesRepository = new SoldesRepository(chemins, store);
            var dialogService = new DialogService();

            var mainWindowViewModel = new MainWindowViewModel(
                configRepository, validationsRepository, postesRepository, achatsRepository, soldesRepository, dialogService);

            var mainWindow = new MainWindow { DataContext = mainWindowViewModel };
            mainWindow.Show();
        }
        catch (Exception ex)
        {
            var cheminLog = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "alxbudgetdesktop-crash.log");
            System.IO.File.WriteAllText(cheminLog, ex.ToString());
            MessageBox.Show($"Erreur au démarrage :\n\n{ex}\n\n(détail écrit dans {cheminLog})", "alxBudgetDesktop");
            throw;
        }
    }
}
