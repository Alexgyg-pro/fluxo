using System.Windows;
using Fluxo.App.Mvvm;
using Fluxo.App.ViewModels.Shell;
using Fluxo.App.Views.Shell;
using Fluxo.Data.Csv;
using Fluxo.Data.Json;
using Fluxo.Data.Paths;
using Fluxo.Data.Repositories;

namespace Fluxo.App;

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
            var fenetreRepository = new FenetreRepository(chemins, store);
            var preferencesRepository = new PreferencesRepository(chemins, store);
            var dialogService = new DialogService();

            var mainWindowViewModel = new MainWindowViewModel(
                configRepository, validationsRepository, postesRepository, achatsRepository, soldesRepository,
                preferencesRepository, dialogService);

            var mainWindow = new MainWindow { DataContext = mainWindowViewModel };
            AppliquerFenetreEnregistree(mainWindow, fenetreRepository.Charger());
            mainWindow.Closing += (_, _) => fenetreRepository.Enregistrer(CapturerFenetre(mainWindow));
            mainWindow.Show();
        }
        catch (Exception ex)
        {
            var cheminLog = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "fluxo-crash.log");
            System.IO.File.WriteAllText(cheminLog, ex.ToString());
            MessageBox.Show($"Erreur au démarrage :\n\n{ex}\n\n(détail écrit dans {cheminLog})", "Fluxo");
            throw;
        }
    }

    /// <summary>Restaure la taille/position de la fenêtre depuis la session précédente.
    /// Si aucune position n'a jamais été enregistrée (premier lancement), la fenêtre
    /// garde le comportement par défaut défini dans MainWindow.xaml (CenterScreen).</summary>
    private static void AppliquerFenetreEnregistree(Window fenetre, FenetreParametres parametres)
    {
        fenetre.Width = parametres.Largeur;
        fenetre.Height = parametres.Hauteur;

        if (parametres.Gauche.HasValue && parametres.Haut.HasValue)
        {
            fenetre.WindowStartupLocation = WindowStartupLocation.Manual;
            fenetre.Left = parametres.Gauche.Value;
            fenetre.Top = parametres.Haut.Value;
        }

        if (parametres.Maximisee)
        {
            fenetre.WindowState = WindowState.Maximized;
        }
    }

    private static FenetreParametres CapturerFenetre(Window fenetre)
    {
        var maximisee = fenetre.WindowState == WindowState.Maximized;
        var limites = maximisee ? fenetre.RestoreBounds : new Rect(fenetre.Left, fenetre.Top, fenetre.Width, fenetre.Height);

        return new FenetreParametres
        {
            Largeur = limites.Width,
            Hauteur = limites.Height,
            Gauche = limites.Left,
            Haut = limites.Top,
            Maximisee = maximisee,
        };
    }
}
