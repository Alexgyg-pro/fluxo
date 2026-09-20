# Backlog — alxBudgetDesktop

> Méthode : socle livré en mode classique (phases), puis itérations agiles.
> Les nouveaux éléments s'ajoutent **en tête** de leur section.

---

## En cours

### US-004 — Créer un installateur
**En tant qu'**utilisateur, **je veux** un véritable installateur Windows (au lieu d'un dossier `publish/` à copier manuellement)
**afin de** pouvoir installer l'application proprement moi-même, et surtout la partager avec quelqu'un d'autre de façon présentable, si l'occasion se présente.

**Contexte :** Le packaging actuel (`dotnet publish` single-file self-contained + `README-utilisateur.txt`) fonctionne mais reste un geste de développeur — copier un dossier, éventuellement créer soi-même un raccourci Bureau avec la bonne icône. Pas rédhibitoire pour un usage strictement personnel, mais peu engageant pour quelqu'un d'externe.

**Pistes techniques à trancher à la reprise (décision PO, pas avant) :**
- Outil : MSIX (natif Windows, signature simplifiée, désinstallation propre via "Applications" Windows) vs Inno Setup / WiX (installateur `.exe`/`.msi` plus classique, pas de contraintes de signature aussi strictes).
- Raccourci Bureau + menu Démarrer créés automatiquement, avec l'icône déjà existante (`icone.ico`).
- Emplacement des données par défaut à l'installation : garder `%LOCALAPPDATA%\alxBudgetDesktop\sample-data` (cohérent avec le comportement actuel du `DataPathProvider`, pas de régression à prévoir).
- Désinstallation propre (registre "Programmes et fonctionnalités"), en clarifiant si elle doit ou non supprimer les données utilisateur (probable : non, par défaut, pour ne pas perdre de données par erreur).
- Signature de l'exécutable/installateur : probablement hors de portée pour un usage perso (certificat payant) — à documenter comme limite connue (SmartScreen affichera un avertissement à l'installation) plutôt qu'à résoudre.

**Conditions de réussite (à affiner à la reprise) :**
- [ ] Décision PO sur l'outil retenu.
- [ ] Un seul fichier à exécuter installe l'application, crée le raccourci (Bureau et/ou menu Démarrer) avec la bonne icône, sans étape manuelle.
- [ ] Désinstallation propre via les outils Windows standards.
- [ ] Testé sur une machine "propre" si possible (sans le SDK .NET ni l'environnement de dev), pour valider que le self-contained fonctionne réellement sans dépendance cachée.

---

### US-003 — Valider le contrôle de rapprochement en conditions réelles
**En tant qu'**utilisateur, **je veux** tester le contrôle de rapprochement bancaire (bouton "Contrôler avec un relevé") avec un vrai export CSV de ma banque
**afin de** m'assurer qu'il fonctionne correctement sur mes propres relevés, pas seulement sur des fixtures synthétiques.

**Contexte :** Depuis plusieurs semaines, le PO n'arrive plus à télécharger l'export CSV depuis le site de sa banque (CIC) — le lien mentionné dans NOTICE.md d'alxBudget ("Opérations" → "Télécharger vos dernières opérations") ne fonctionne plus de son côté. Cette fonctionnalité n'a donc jamais pu être testée en conditions réelles, ni côté alxBudget (web) ni côté alxBudgetDesktop. Le code (parseur CSV CIC, algorithme de rapprochement, contrôle éphémère) a été porté fidèlement depuis alxBudget et testé unitairement (32 tests domaine) et avec un CSV synthétique, mais jamais validé sur un vrai relevé.

**Décision PO :** mis en pause, pas bloquant pour la suite du développement. À reprendre dès que l'export CSV redevient accessible. Si la banque a définitivement retiré cette fonctionnalité côté site, il faudra revoir l'approche (autre format d'export si disponible, ou abandon du rapprochement automatique au profit d'une vérification manuelle des opérations) — décision à prendre avec le PO à ce moment-là, pas avant.

**Conditions de réussite (à affiner à la reprise) :**
- [ ] Un export CSV réel de la banque a pu être obtenu.
- [ ] Le contrôle de rapprochement testé dessus donne des résultats cohérents (matches, orphelines, à saisir) validés manuellement par le PO.
- [ ] Si l'export CSV n'est plus disponible : décision PO consignée sur l'alternative retenue.

---

## Terminé

### US-002 — Chaînage du solde d'ouverture entre années
**En tant qu'**utilisateur, **je veux** que le solde d'ouverture de janvier reprenne la clôture de décembre de l'année précédente (quand elle existe)
**afin de** avoir des soldes corrects dès la première consultation d'une nouvelle année, sans devoir caler manuellement chaque janvier.

**Décisions PO (livrées) :**
- Demandé par anticipation par le PO, avant même d'avoir une deuxième année de données réelles, pour ne pas avoir à y repenser en fin d'année.
- `SoldesAnneeService` (nouveau, côté `Data`) orchestre `AchatsRepository`/`PostesRepository`/`SoldesRepository` pour remonter récursivement l'ouverture de janvier sur la clôture de décembre de l'année précédente, tant qu'une année antérieure a des données (achats ou calage), avec un plancher à l'année 2000 — port fidèle de `routes/soldes.ts::ouvertureJanvierDe` côté alxBudget. `SoldesService` (Domain) reste pur, inchangé.
- Nouveau projet `AlxBudgetDesktop.Data.Tests` (5 tests, dossiers temporaires isolés) : sans année précédente, chaînage sur une année, chaînage récursif sur plusieurs années, propagation d'un calage manuel, comportement au plancher 2000.

**Conditions de réussite :**
- [x] Une fonction équivalente à `ouvertureJanvierDe` (remontée récursive, plancher 2000) ajoutée côté `Data`.
- [x] `MoisViewModel` l'utilise pour calculer l'ouverture de janvier.
- [x] Vérifié avec au moins deux années de données synthétiques successives (tests automatisés).

**Livré le :** 2026-09-20.

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
