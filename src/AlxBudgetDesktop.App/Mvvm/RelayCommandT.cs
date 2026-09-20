using System.Windows.Input;

namespace AlxBudgetDesktop.App.Mvvm;

/// <summary>Variante paramétrée de <see cref="RelayCommand"/> (ex. supprimer une ligne précise).</summary>
public sealed class RelayCommand<T>(Action<T?> execute, Func<T?, bool>? canExecute = null) : ICommand
{
    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public bool CanExecute(object? parameter) => canExecute?.Invoke((T?)parameter) ?? true;

    public void Execute(object? parameter) => execute((T?)parameter);

    public void RaiseCanExecuteChanged() => CommandManager.InvalidateRequerySuggested();
}
