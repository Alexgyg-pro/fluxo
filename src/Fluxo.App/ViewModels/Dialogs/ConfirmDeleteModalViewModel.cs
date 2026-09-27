using Fluxo.App.Mvvm;

namespace Fluxo.App.ViewModels.Dialogs;

/// <summary>Modale applicative de confirmation de suppression (pas de MessageBox natif).</summary>
public sealed class ConfirmDeleteModalViewModel : ModalViewModelBase
{
    public string Titre { get; }
    public string Etiquette { get; }
    public decimal Montant { get; }

    public RelayCommand ConfirmerCommand { get; }
    public RelayCommand AnnulerCommand { get; }

    public ConfirmDeleteModalViewModel(bool estRecette, string etiquette, decimal montant)
    {
        Titre = estRecette ? "Supprimer cette recette ?" : "Supprimer cette dépense ?";
        Etiquette = etiquette;
        Montant = montant;

        ConfirmerCommand = new RelayCommand(() => Close(true));
        AnnulerCommand = new RelayCommand(() => Close(false));
    }
}
