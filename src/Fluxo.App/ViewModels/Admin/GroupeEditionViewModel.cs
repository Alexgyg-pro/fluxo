using System.Collections.ObjectModel;
using Fluxo.App.Mvvm;

namespace Fluxo.App.ViewModels.Admin;

/// <summary>Un groupe de postes en cours d'édition dans le brouillon de l'onglet Admin
/// (nom éditable inline, postes ajoutables/supprimables). Rien n'est persisté tant que
/// l'utilisateur n'a pas cliqué sur "Enregistrer les postes".</summary>
public sealed class GroupeEditionViewModel : ObservableObject
{
    private string _nom;
    public string Nom { get => _nom; set => SetProperty(ref _nom, value); }

    public ObservableCollection<PosteEditionViewModel> Postes { get; } = [];

    public RelayCommand AjouterPosteCommand { get; }
    public RelayCommand<PosteEditionViewModel> SupprimerPosteCommand { get; }

    public GroupeEditionViewModel(string nom)
    {
        _nom = nom;
        AjouterPosteCommand = new RelayCommand(() => Postes.Add(new PosteEditionViewModel("")));
        SupprimerPosteCommand = new RelayCommand<PosteEditionViewModel>(poste =>
        {
            if (poste is not null)
            {
                Postes.Remove(poste);
            }
        });
    }
}
