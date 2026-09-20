using AlxBudgetDesktop.Domain.Models;

namespace AlxBudgetDesktop.Domain.Services;

/// <summary>
/// Port de validation.ts. Le format des dates et la validité de l'énumération
/// Compte sont garantis par le système de types C# (DateOnly, enum Compte) et
/// n'ont donc pas besoin d'être revalidés ici, contrairement au TypeScript qui
/// travaille sur des chaînes/objets non typés à cette étape.
/// </summary>
public static class AchatValidator
{
    public static IReadOnlyList<string> Valider(AchatInput input)
    {
        var erreurs = new List<string>();

        if (string.IsNullOrWhiteSpace(input.Etiquette))
        {
            erreurs.Add("L'étiquette est obligatoire.");
        }

        if (input.MontantAchat <= 0)
        {
            erreurs.Add("Le montant doit être strictement positif.");
        }

        if (input.Postes.Count == 0)
        {
            erreurs.Add("Au moins un poste doit être renseigné.");
        }

        var sommePostes = Arrondi.Arrondi2(input.Postes.Values.Sum());
        var montant = Arrondi.Arrondi2(input.MontantAchat);
        if (sommePostes != montant)
        {
            erreurs.Add($"La somme des postes ({sommePostes:0.00}) doit être égale au montant ({montant:0.00}).");
        }

        return erreurs;
    }
}
