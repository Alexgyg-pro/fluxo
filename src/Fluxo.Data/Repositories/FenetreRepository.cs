using Fluxo.Data.Json;
using Fluxo.Data.Paths;

namespace Fluxo.Data.Repositories;

/// <summary>Dimensions/position de la fenêtre principale, mémorisées d'une session à l'autre.</summary>
public sealed class FenetreParametres
{
    public double Largeur { get; init; } = 1100;
    public double Hauteur { get; init; } = 720;
    public double? Gauche { get; init; }
    public double? Haut { get; init; }
    public bool Maximisee { get; init; }
}

public sealed class FenetreRepository(DataPathProvider chemins, JsonFileStore store)
{
    private string Chemin => chemins.CheminFichier("fenetre.json");

    public FenetreParametres Charger() => store.LoadOrCreateJson(Chemin, () => new FenetreParametres());

    public void Enregistrer(FenetreParametres parametres) => store.SaveJsonAtomic(Chemin, parametres);
}
