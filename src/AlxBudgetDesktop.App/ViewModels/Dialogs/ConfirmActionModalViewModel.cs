using AlxBudgetDesktop.App.Mvvm;

namespace AlxBudgetDesktop.App.ViewModels.Dialogs;

/// <summary>Modale applicative de confirmation générique (pas de MessageBox natif) —
/// contrairement à ConfirmDeleteModalViewModel, aucun wording de suppression en dur :
/// réutilisable pour n'importe quelle action à confirmer (ex. réinitialiser un contrôle).</summary>
public sealed class ConfirmActionModalViewModel : ModalViewModelBase
{
    public string Titre { get; }
    public string Message { get; }
    public string TexteBoutonConfirmer { get; }

    public RelayCommand ConfirmerCommand { get; }
    public RelayCommand AnnulerCommand { get; }

    public ConfirmActionModalViewModel(string titre, string message, string texteBoutonConfirmer)
    {
        Titre = titre;
        Message = message;
        TexteBoutonConfirmer = texteBoutonConfirmer;

        ConfirmerCommand = new RelayCommand(() => Close(true));
        AnnulerCommand = new RelayCommand(() => Close(false));
    }
}
