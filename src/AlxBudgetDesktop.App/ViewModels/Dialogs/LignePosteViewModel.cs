using AlxBudgetDesktop.App.Mvvm;
using AlxBudgetDesktop.Domain;
using AlxBudgetDesktop.Domain.Models;

namespace AlxBudgetDesktop.App.ViewModels.Dialogs;

/// <summary>
/// Une ligne de répartition poste/montant dans la modale de saisie. Le montant se
/// saisit toujours en positif, y compris pour un poste du groupe Remises — c'est
/// <see cref="MontantEffectif"/> qui applique le signe négatif pour le calcul de la
/// somme, reproduisant la règle des cagnottes/réductions d'alxBudget.
/// </summary>
public sealed class LignePosteViewModel(IReadOnlyList<GroupePostes> groupes) : ObservableObject
{
    private string? _posteSelectionne;
    public string? PosteSelectionne
    {
        get => _posteSelectionne;
        set
        {
            if (SetProperty(ref _posteSelectionne, value))
            {
                OnPropertyChanged(nameof(EstRemise));
            }
        }
    }

    private string _montantTexte = "";
    public string MontantTexte
    {
        get => _montantTexte;
        set => SetProperty(ref _montantTexte, value);
    }

    public bool EstRemise
        => _posteSelectionne is not null
           && groupes.FirstOrDefault(g => g.Nom == PostesSpeciaux.GroupeRemises)?.Postes.Contains(_posteSelectionne) == true;

    public decimal MontantSaisi => decimal.TryParse(MontantTexte.Replace(',', '.'),
        System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out var v) ? v : 0m;

    /// <summary>Montant réellement pris en compte dans la somme des postes (négatif si remise).</summary>
    public decimal MontantEffectif => EstRemise ? -MontantSaisi : MontantSaisi;
}
