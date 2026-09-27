using System.Windows.Input;

namespace Fluxo.App.Mvvm;

/// <summary>Commande sans paramètre. S'appuie sur CommandManager.RequerySuggested pour
/// une réévaluation automatique de CanExecute lors des interactions standard (focus, clic),
/// complétée par RaiseCanExecuteChanged() pour les cas où le ViewModel change un état
/// en dehors d'une interaction utilisateur directe.</summary>
public sealed class RelayCommand(Action execute, Func<bool>? canExecute = null) : ICommand
{
    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public bool CanExecute(object? parameter) => canExecute?.Invoke() ?? true;

    public void Execute(object? parameter) => execute();

    public void RaiseCanExecuteChanged() => CommandManager.InvalidateRequerySuggested();
}
