# Prompt initial — alxBudgetDesktop

## Contexte et objectif

alxBudgetDesktop est le **portage en application de bureau** d'un projet existant,
**alxBudget** (application web locale de gestion de budget personnel : Node/Express +
TypeScript côté serveur, React + TypeScript côté client), situé sur cette même machine à :

```
D:\Alejandro\developpement\alexprojects\ai\alxBudget
```

Objectifs du portage, par ordre d'importance :

1. **Confort d'usage** : lancer un simple exécutable Windows plutôt que devoir démarrer un
   serveur Node et ouvrir un navigateur.
2. **Apprentissage** : l'auteur (Product Owner sur ce projet, comme sur alxBudget) est
   développeur débutant et souhaite se remettre à niveau en C#/.NET à travers ce projet.

Ce n'est **pas** une réécriture libre : le comportement fonctionnel doit reproduire fidèlement
celui d'alxBudget (mêmes règles métier, même modèle de données, mêmes écrans dans l'esprit).
Le code source TypeScript d'alxBudget fait foi en cas de doute sur une règle précise — il est
plus fiable qu'une description approximative dans ce document.

## Référence fonctionnelle — à lire avant de commencer

Avant toute conception, lire dans le dépôt alxBudget :

- **`NOTICE.md`** (racine du projet) : mode d'emploi utilisateur complet, décrit tous les
  écrans et gestes courants du point de vue fonctionnel.
- **`BACKLOG.md`** (racine du projet), section **« Terminé »** : liste exhaustive des user
  stories livrées, avec leurs décisions PO et conditions de réussite — c'est la spécification
  la plus précise et la plus à jour de chaque fonctionnalité.
- Le domaine métier en TypeScript, à relire pour chaque algorithme avant de le porter :
  - `server/src/domain/validation.ts` — règle « somme des postes = montant ».
  - `server/src/domain/reconciliation.ts` — algorithme de rapprochement bancaire.
  - `server/src/domain/budget.ts` — calcul des projections et du tableau Budget.
  - `server/src/domain/soldes.ts` — calcul des soldes du mois (report/calage/disponible).
  - `server/src/domain/csv/cic.ts` — parseur du CSV CIC.
  - `server/src/store/defaults.ts` — postes/groupes par défaut et noms spéciaux
    (`POSTES_SPECIAUX` : Revenus, Épargne, Remises, Tickets restaurant).
  - `server/src/types.ts` — modèle de données complet (`Achat`, `GroupePostes`,
    `SoldesComptes`, `ResultatRapprochement`, `BudgetResult`, etc.).

Ne pas hésiter à ouvrir ces fichiers directement pendant le développement plutôt que de
travailler uniquement sur la base de ce prompt.

## Choix techniques imposés (déjà tranchés avec le PO)

- **UI : WPF** (.NET 8 ou plus récent LTS disponible), pattern **MVVM**.
- **Distribution : exécutable auto-suffisant**, publié en single-file
  (`dotnet publish -r win-x64 --self-contained true -p:PublishSingleFile=true`) — aucune
  installation de runtime .NET requise sur la machine cible.
- **Stockage : mêmes formats et même arborescence de fichiers qu'alxBudget** — JSON /
  JSONL, un fichier par année pour les achats, via `System.Text.Json` :
  - `achats-<année>.jsonl` (un achat par ligne)
  - `postes.json`, `beneficiaires.json`, `config.json`
  - `soldes-<année>.json`, `validations-<année>.json`
  - Reprendre exactement les mêmes structures de champs que `server/src/types.ts`, pour
    permettre à terme de pointer l'appli C# sur le dossier `data/` réel d'alxBudget sans
    migration.
- **Sécurité des données réelles** : le dossier `data/` d'alxBudget contient de vraies
  données financières en production depuis le 2026-07-01. **Développer et tester
  exclusivement sur un jeu de données séparé** (dossier de test ou données synthétiques,
  chemin configurable). Ne pointer l'application vers le dossier `data/` réel qu'une fois
  celle-ci validée par le PO à l'usage — jamais avant.

## Méthodologie

Même approche qu'alxBudget (voir son historique) : un **socle en mode classique** (phases),
puis bascule en **backlog agile** (`BACKLOG.md`, sections « En cours » / « Terminé »,
phrasé « en tant que… je veux… afin de… », décisions PO consignées **avant** le code).
Respecter aussi les conventions déjà définies dans le `CLAUDE.md` global de l'auteur (politique
Git main/develop/feature, format `AIPLAN.md`, ne jamais lancer de process bloquant depuis
Claude Code).

## Portée de la version 1 (fonctionnalités à porter)

Tout ce qui est **livré et validé** dans `BACKLOG.md` (section Terminé) au moment de la
rédaction de ce prompt, soit dans l'esprit :

1. **Saisie manuelle** d'une dépense ou d'une recette (fenêtre modale), avec répartition sur
   plusieurs postes et règle « somme des postes = montant » vérifiée en direct.
2. **Groupes de postes spéciaux** reconnus par leur nom : Revenus (classe l'entrée en
   recette), Épargne (dépôt/retrait), Remises (postes soustractifs, saisis en positif,
   déduits automatiquement — cf. règle des cagnottes/réductions).
3. **Import CSV CIC** et rapprochement bancaire (montant + date de prélèvement ≥ date
   d'achat, matching un-à-un, départage par date la plus proche, anti-doublon par référence
   de transaction), registre des bénéficiaires (suggestion de poste par raison sociale déjà
   rencontrée).
4. **Contrôle de rapprochement de fin de mois**, indépendant de l'import : badges
   « orpheline » (saisie sans ligne au relevé) et « à saisir » (ligne du relevé sans saisie,
   affichée en ligne fantôme cliquable → saisie pré-remplie). Ne porte que sur les dépenses
   du compte courant.
5. **Validation/dévalidation d'un mois** (marqueur manuel, actif seulement à 0 écart de
   rapprochement), pastille sur l'onglet du mois concerné.
6. **Soldes du mois** (compte courant, tickets restaurant, épargne) : report automatique en
   chaîne depuis la clôture du mois précédent, calage manuel possible avec aperçu du report
   et de la correction en direct.
7. **Onglet Budget** : tableau annuel par poste et par groupe, montants réels pour les mois
   passés/en cours, projection pour les mois futurs (moyenne des seuls mois **passés et
   renseignés**, mois courant toujours exclu ; un poste sans historique reste vide/« – » sur
   ses mois futurs). Colonne **Total annuel** par ligne et par groupe. Ligne **Solde** en bas
   de tableau (Revenus − tous les autres groupes, Remises comprises), avec son propre total
   annuel.
8. **Onglet Admin** : changer l'année gérée par l'application (un fichier de données distinct
   par année) ; éditer les groupes et postes (ajout/renommage/suppression), avec confirmation
   (fenêtre de dialogue) après enregistrement réussi, message d'erreur clair en cas d'échec.
9. **Suppression d'une opération** avec confirmation dans une fenêtre applicative cohérente
   avec le reste de l'UI (pas de MessageBox système brute — s'inspirer du style visuel des
   autres fenêtres de l'application).

## Hors scope pour la v1

- **US-016 / US-017** d'alxBudget : encore « En cours » côté alxBudget au moment de ce
  prompt (projection sur mois validés uniquement ; fichiers JSON par défaut) — à ignorer
  pour l'instant, elles n'existent pas encore côté fonctionnel de référence.
- **Import de relevés PDF** : jamais implémenté côté alxBudget (évalué mais mis de côté :
  complexité de l'extraction des colonnes Débit/Crédit, pas de référence anti-doublon
  fiable). Ne pas s'y engager sans validation explicite du PO.
- Toute fonctionnalité non listée ci-dessus et non présente dans `NOTICE.md` /
  `BACKLOG.md` (section Terminé).
