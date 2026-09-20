# Backlog — alxBudgetDesktop

> Méthode : socle livré en mode classique (phases), puis itérations agiles.
> Les nouveaux éléments s'ajoutent **en tête** de leur section.

---

## En cours

### US-002 — Chaînage du solde d'ouverture entre années
**En tant qu'**utilisateur, **je veux** que le solde d'ouverture de janvier reprenne la clôture de décembre de l'année précédente (quand elle existe)
**afin de** avoir des soldes corrects dès la première consultation d'une nouvelle année, sans devoir caler manuellement chaque janvier.

**Contexte :** `MoisViewModel.Charger()` appelle `SoldesService.CalculerSoldesAnnee(Annee, achatsAnnee, calages, groupes)` sans préciser `ouvertureJanvier`, qui vaut donc toujours zéro (défaut du service de domaine). Côté alxBudget (référence), ce chaînage inter-années est calculé par la couche route (`routes/soldes.ts::ouvertureJanvierDe`), récursivement, avec un plancher à l'année 2000 — cette couche n'a pas encore d'équivalent ici.

**Décision PO :** reporté à la session qui développera le calage manuel des soldes (`CalageModalWindow`, cf. plan des écrans) — les deux sujets touchent la même zone (persistance et calcul des soldes) et gagnent à être traités ensemble.

**Conditions de réussite (à affiner à la reprise) :**
- [ ] Une fonction équivalente à `ouvertureJanvierDe` (remontée récursive tant qu'une année précédente a des données, plancher 2000) est ajoutée côté `Data` ou `App`.
- [ ] `MoisViewModel` (ou son successeur) l'utilise pour calculer l'ouverture de janvier.
- [ ] Vérifié avec au moins deux années de données synthétiques successives.

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
