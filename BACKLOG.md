# Backlog — alxBudgetDesktop

> Méthode : socle livré en mode classique (phases), puis itérations agiles.
> Les nouveaux éléments s'ajoutent **en tête** de leur section.

---

## En cours

### US-003 — Valider le contrôle de rapprochement en conditions réelles
**En tant qu'**utilisateur, **je veux** tester le contrôle de rapprochement bancaire (bouton "Contrôler avec un relevé") avec un vrai export CSV de ma banque
**afin de** m'assurer qu'il fonctionne correctement sur mes propres relevés, pas seulement sur des fixtures synthétiques.

**Contexte :** Depuis plusieurs semaines, le PO n'arrive plus à télécharger l'export CSV depuis le site de sa banque (CIC) — le lien mentionné dans NOTICE.md d'alxBudget ("Opérations" → "Télécharger vos dernières opérations") ne fonctionne plus de son côté. Cette fonctionnalité n'a donc jamais pu être testée en conditions réelles, ni côté alxBudget (web) ni côté alxBudgetDesktop. Le code (parseur CSV CIC, algorithme de rapprochement, contrôle éphémère) a été porté fidèlement depuis alxBudget et testé unitairement (32 tests domaine) et avec un CSV synthétique, mais jamais validé sur un vrai relevé.

**Décision PO :** mis en pause, pas bloquant pour la suite du développement. À reprendre dès que l'export CSV redevient accessible. Si la banque a définitivement retiré cette fonctionnalité côté site, il faudra revoir l'approche (autre format d'export si disponible, ou abandon du rapprochement automatique au profit d'une vérification manuelle des opérations) — décision à prendre avec le PO à ce moment-là, pas avant.

**Conditions de réussite (à affiner à la reprise) :**
- [ ] Un export CSV réel de la banque a pu être obtenu.
- [ ] Le contrôle de rapprochement testé dessus donne des résultats cohérents (matches, orphelines, à saisir) validés manuellement par le PO.
- [ ] Si l'export CSV n'est plus disponible : décision PO consignée sur l'alternative retenue.

### US-002 — Chaînage du solde d'ouverture entre années
**En tant qu'**utilisateur, **je veux** que le solde d'ouverture de janvier reprenne la clôture de décembre de l'année précédente (quand elle existe)
**afin de** avoir des soldes corrects dès la première consultation d'une nouvelle année, sans devoir caler manuellement chaque janvier.

**Contexte :** `MoisViewModel.Charger()` appelle `SoldesService.CalculerSoldesAnnee(Annee, achatsAnnee, calages, groupes)` sans préciser `ouvertureJanvier`, qui vaut donc toujours zéro (défaut du service de domaine). Côté alxBudget (référence), ce chaînage inter-années est calculé par la couche route (`routes/soldes.ts::ouvertureJanvierDe`), récursivement, avec un plancher à l'année 2000 — cette couche n'a pas encore d'équivalent ici.

**Décision PO :** reporté à la session qui développera le calage manuel des soldes (`CalageModalWindow`, cf. plan des écrans) — les deux sujets touchent la même zone (persistance et calcul des soldes) et gagnent à être traités ensemble.

**Conditions de réussite (à affiner à la reprise) :**
- [ ] Une fonction équivalente à `ouvertureJanvierDe` (remontée récursive tant qu'une année précédente a des données, plancher 2000) est ajoutée côté `Data` ou `App`.
- [ ] `MoisViewModel` (ou son successeur) l'utilise pour calculer l'ouverture de janvier.
- [ ] Vérifié avec au moins deux années de données synthétiques successives.

---

## Terminé

### US-001 — Double sélection visuelle dans la barre d'onglets
**En tant qu'**utilisateur, **je veux** qu'un seul onglet apparaisse sélectionné à la fois dans la barre du shell
**afin de** ne pas être dérouté par un état visuel ambigu (un mois et une commande Budget/Import/Admin semblant sélectionnés simultanément).

**Décisions PO (livrées) :**
- Cause racine identifiée grâce à un scénario de repro détaillé du PO (Budget → clic sur un mois sans effet → il fallait cliquer un *autre* mois d'abord). Deux `ListBox` liées en `TwoWay` à la même propriété `OngletSelectionne` : quand la valeur passe à un objet absent de la propre `ItemsSource` d'une des deux listes, WPF ne vide PAS sa sélection interne — elle garde l'ancien item "collé", ce qui bloquait aussi les clics suivants sur ce même item.
- Correctif : abandon complet du mécanisme `ListBox`/`Selector` pour la barre d'onglets, remplacé par `ItemsControl` + `Button`, sélection pilotée explicitement par `MainWindowViewModel` (`TabViewModelBase.EstSelectionne`, mis à jour sur l'ancien ET le nouvel onglet à chaque changement, via `SelectionnerOngletCommand`).

**Conditions de réussite :**
- [x] Cause racine identifiée.
- [x] Un seul onglet visuellement en surbrillance à la fois, quel que soit l'ordre des clics entre le groupe mois et le groupe Budget/Import/Admin.
- [x] Validation visuelle par le PO.

**Livré le :** 2026-09-20 — validé à l'usage par le PO.
