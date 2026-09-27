using Fluxo.App.ViewModels.Aide;
using Fluxo.App.ViewModels.Dialogs;

namespace Fluxo.App.Mvvm;

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

    /// <summary>Modale de confirmation générique (succès/échec), fermée par OK.</summary>
    void ShowConfirmation(ConfirmationModalViewModel viewModel);

    /// <summary>true si l'action a été confirmée. Contrairement à ShowConfirmDelete, aucun
    /// wording de suppression en dur — pour n'importe quelle action à confirmer.</summary>
    bool ShowConfirmAction(ConfirmActionModalViewModel viewModel);

    /// <summary>Ouvre la fenêtre d'aide — non modale, contrairement aux autres méthodes
    /// ci-dessus : on doit pouvoir la garder ouverte en consultant le reste de l'appli.</summary>
    void ShowAide(AideViewModel viewModel);
}
