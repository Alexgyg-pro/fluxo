using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace AlxBudgetDesktop.App.Aide;

/// <summary>
/// Convertit un sous-ensemble volontairement limité de Markdown (titres # / ##,
/// listes "- ", **gras**, paragraphes) en FlowDocument WPF, pour l'aide en ligne.
/// Ce n'est pas un moteur Markdown complet : le contenu de l'aide est écrit à la
/// main pour ce besoin précis, pas de tables/liens/images à gérer.
/// </summary>
public static class MarkdownVersFlowDocument
{
    public static FlowDocument Convertir(string markdown)
    {
        var document = new FlowDocument
        {
            FontFamily = new FontFamily("Segoe UI"),
            FontSize = 14,
            PagePadding = new Thickness(0),
        };

        List? listeCourante = null;

        foreach (var ligneBrute in markdown.Replace("\r\n", "\n").Split('\n'))
        {
            var ligne = ligneBrute.TrimEnd();

            if (ligne.StartsWith("## "))
            {
                listeCourante = null;
                document.Blocks.Add(CreerTitre(ligne[3..], 17));
            }
            else if (ligne.StartsWith("# "))
            {
                listeCourante = null;
                document.Blocks.Add(CreerTitre(ligne[2..], 22));
            }
            else if (ligne.StartsWith("- "))
            {
                if (listeCourante is null)
                {
                    listeCourante = new List { Margin = new Thickness(0, 4, 0, 8) };
                    document.Blocks.Add(listeCourante);
                }

                var itemParagraphe = new Paragraph();
                AjouterTexteAvecGras(itemParagraphe, ligne[2..]);
                listeCourante.ListItems.Add(new ListItem(itemParagraphe));
            }
            else if (string.IsNullOrWhiteSpace(ligne))
            {
                listeCourante = null;
            }
            else
            {
                listeCourante = null;
                var paragraphe = new Paragraph { Margin = new Thickness(0, 0, 0, 8) };
                AjouterTexteAvecGras(paragraphe, ligne);
                document.Blocks.Add(paragraphe);
            }
        }

        return document;
    }

    private static Paragraph CreerTitre(string texte, double taille)
        => new(new Run(texte)) { FontSize = taille, FontWeight = FontWeights.Bold, Margin = new Thickness(0, 12, 0, 8) };

    /// <summary>Découpe une ligne sur "**" : segments d'indice impair = gras.</summary>
    private static void AjouterTexteAvecGras(Paragraph paragraphe, string texte)
    {
        var segments = texte.Split("**");
        for (var i = 0; i < segments.Length; i++)
        {
            if (segments[i].Length == 0)
            {
                continue;
            }

            paragraphe.Inlines.Add(new Run(segments[i]) { FontWeight = i % 2 == 1 ? FontWeights.Bold : FontWeights.Normal });
        }
    }
}
