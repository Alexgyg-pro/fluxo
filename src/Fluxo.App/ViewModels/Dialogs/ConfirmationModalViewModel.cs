using Fluxo.App.Mvvm;

namespace Fluxo.App.ViewModels.Dialogs;

/// <summary>Modale applicative de confirmation générique (succès/échec) — pas de
/// MessageBox natif. Utilisée par l'onglet Admin après "Enregistrer les postes".</summary>
public sealed class ConfirmationModalViewModel : ModalViewModelBase
{
    public string Titre { get; }
    public string Message { get; }
    public bool EstSucces { get; }

    public RelayCommand OkCommand { get; }

    public ConfirmationModalViewModel(string titre, string message, bool estSucces)
    {
        Titre = titre;
        Message = message;
        EstSucces = estSucces;
        OkCommand = new RelayCommand(() => Close(true));
    }
}
