using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AlxBudgetDesktop.App.Mvvm;

/// <summary>
/// Classe de base pour les ViewModels : implémente INotifyPropertyChanged à la main
/// (pas de générateur de source type CommunityToolkit.Mvvm) — choix pédagogique pour
/// comprendre le mécanisme de liaison de données WPF plutôt que le masquer.
/// </summary>
public abstract class ObservableObject : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected bool SetProperty<T>(ref T champ, T valeur, [CallerMemberName] string? nomPropriete = null)
    {
        if (EqualityComparer<T>.Default.Equals(champ, valeur))
        {
            return false;
        }

        champ = valeur;
        OnPropertyChanged(nomPropriete);
        return true;
    }

    protected void OnPropertyChanged([CallerMemberName] string? nomPropriete = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nomPropriete));
}
