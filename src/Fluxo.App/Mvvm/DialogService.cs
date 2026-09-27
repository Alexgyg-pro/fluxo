using System.Windows;
using Fluxo.App.ViewModels.Aide;
using Fluxo.App.ViewModels.Dialogs;
using Fluxo.App.Views.Aide;
using Fluxo.App.Views.Dialogs;

namespace Fluxo.App.Mvvm;

public sealed class DialogService : IDialogService
{
    public bool ShowAchatModal(AchatModalViewModel viewModel)
        => ShowModal(new AchatModalWindow { DataContext = viewModel }, viewModel);

    public bool ShowConfirmDelete(ConfirmDeleteModalViewModel viewModel)
        => ShowModal(new ConfirmDeleteModalWindow { DataContext = viewModel }, viewModel);

    public bool ShowCalageModal(CalageModalViewModel viewModel)
        => ShowModal(new CalageModalWindow { DataContext = viewModel }, viewModel);

    public void ShowConfirmation(ConfirmationModalViewModel viewModel)
        => ShowModal(new ConfirmationModalWindow { DataContext = viewModel }, viewModel);

    public bool ShowConfirmAction(ConfirmActionModalViewModel viewModel)
        => ShowModal(new ConfirmActionModalWindow { DataContext = viewModel }, viewModel);

    public void ShowAide(AideViewModel viewModel)
    {
        var fenetre = new AideWindow { DataContext = viewModel, Owner = Application.Current.MainWindow };
        fenetre.Show();
    }

    private static bool ShowModal(Window fenetre, ModalViewModelBase viewModel)
    {
        var resultat = false;
        viewModel.RequestClose += r =>
        {
            resultat = r;
            fenetre.DialogResult = r;
        };

        fenetre.Owner = Application.Current.MainWindow;
        fenetre.ShowDialog();

        return resultat;
    }
}
