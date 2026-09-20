using AlxBudgetDesktop.App.Mvvm;

namespace AlxBudgetDesktop.App.ViewModels.Admin;

/// <summary>Un poste en cours d'édition dans le brouillon de l'onglet Admin (nom éditable inline).</summary>
public sealed class PosteEditionViewModel(string nom) : ObservableObject
{
    private string _nom = nom;
    public string Nom { get => _nom; set => SetProperty(ref _nom, value); }
}
