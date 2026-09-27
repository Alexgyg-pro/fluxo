namespace Fluxo.App.Mvvm;

/// <summary>
/// Base commune aux ViewModels de fenêtre modale : expose un événement de fermeture
/// avec un résultat (true = validé/enregistré, false = annulé), que <see cref="IDialogService"/>
/// écoute pour piloter la fermeture de la Window WPF réelle.
/// </summary>
public abstract class ModalViewModelBase : ObservableObject
{
    public event Action<bool>? RequestClose;

    protected void Close(bool resultat) => RequestClose?.Invoke(resultat);
}
