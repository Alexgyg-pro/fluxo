using AlxBudgetDesktop.App.ViewModels.Dialogs;

namespace AlxBudgetDesktop.App.Mvvm;

/// <summary>
/// Affiche les fenêtres modales de l'application (jamais de MessageBox natif).
/// Le ViewModel de la modale porte déjà toute la logique (validation, enregistrement) ;
/// le service se contente de construire la Window WPF correspondante et de la montrer.
/// </summary>
public interface IDialogService
{
    /// <summary>true si l'achat a été enregistré, false si la modale a été annulée.</summary>
    bool ShowAchatModal(AchatModalViewModel viewModel);

    /// <summary>true si la suppression a été confirmée.</summary>
    bool ShowConfirmDelete(ConfirmDeleteModalViewModel viewModel);

    /// <summary>true si un calage a été enregistré ou supprimé (dans les deux cas, l'appelant doit rafraîchir).</summary>
    bool ShowCalageModal(CalageModalViewModel viewModel);
}
