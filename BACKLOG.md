# Backlog — alxBudgetDesktop

> Méthode : socle livré en mode classique (phases), puis itérations agiles.
> Les nouveaux éléments s'ajoutent **en tête** de leur section.

---

## En cours

### US-001 — Double sélection visuelle dans la barre d'onglets
**En tant qu'**utilisateur, **je veux** qu'un seul onglet apparaisse sélectionné à la fois dans la barre du shell
**afin de** ne pas être dérouté par un état visuel ambigu (un mois et une commande Budget/Import/Admin semblant sélectionnés simultanément).

**Contexte :** La barre d'onglets est composée de deux `ListBox` distinctes (mois à gauche, Budget/Import/Admin à droite) qui partagent la même propriété liée `OngletSelectionne` (`MainWindowViewModel`). Un premier correctif (`FocusVisualStyle=Null` sur les styles d'item, pour écarter le rectangle de focus clavier par défaut de WPF) n'a pas résolu le problème — la double sélection persiste à l'usage.

**Décision PO :** reporté. La cause exacte n'est pas confirmée et le bon correctif dépend probablement de l'architecture qui sera retenue pour les écrans réels à venir (ex. le vrai contenu de l'onglet Mois pourrait changer la façon dont le focus/la sélection circule dans la fenêtre). À reprendre une fois qu'au moins un écran réel (pas un squelette) sera en place, avec du vrai contenu interactif à l'intérieur des onglets.

**Conditions de réussite (à affiner à la reprise) :**
- [ ] Cause racine identifiée (focus WPF, synchronisation `SelectedItem` entre les deux `ListBox`, ou autre).
- [ ] Un seul onglet visuellement en surbrillance à la fois, quel que soit l'ordre des clics entre le groupe mois et le groupe Budget/Import/Admin.
- [ ] Validation visuelle par le PO.

---

## Terminé

*(vide pour l'instant — le socle technique initial est suivi dans AIPLAN.md, pas ici.)*
