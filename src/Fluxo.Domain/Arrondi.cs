namespace Fluxo.Domain;

/// <summary>
/// Arrondi à 2 décimales, appliqué à chaque étape intermédiaire d'un calcul
/// (pas seulement au résultat final) pour reproduire fidèlement le comportement
/// de arrondi() côté alxBudget (TypeScript), qui accumule un arrondi par addition.
/// </summary>
public static class Arrondi
{
    public static decimal Arrondi2(decimal valeur)
        => Math.Round(valeur, 2, MidpointRounding.AwayFromZero);
}
