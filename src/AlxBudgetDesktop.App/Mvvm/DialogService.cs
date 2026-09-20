using System.Windows;
using AlxBudgetDesktop.App.ViewModels.Dialogs;
using AlxBudgetDesktop.App.Views.Dialogs;

namespace AlxBudgetDesktop.App.Mvvm;

public sealed class DialogService : IDialogService
{
    public bool ShowAchatModal(AchatModalViewModel viewModel)
        => ShowModal(new AchatModalWindow { DataContext = viewModel }, viewModel);

    public bool ShowConfirmDelete(ConfirmDeleteModalViewModel viewModel)
        => ShowModal(new ConfirmDeleteModalWindow { DataContext = viewModel }, viewModel);

    public bool ShowCalageModal(CalageModalViewModel viewModel)
        => ShowModal(new CalageModalWindow { DataContext = viewModel }, viewModel);

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
