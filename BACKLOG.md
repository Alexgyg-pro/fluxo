# Backlog — alxBudgetDesktop

> Méthode : socle livré en mode classique (phases), puis itérations agiles.
> Les nouveaux éléments s'ajoutent **en tête** de leur section.

---

## En cours

### US-008 — Rapprochement persisté, cumulable sur plusieurs relevés
**En tant qu'**utilisateur, **je veux** que chaque achat rapproché avec succès soit marqué comme tel de façon durable, pour pouvoir contrôler un mois en plusieurs fois avec plusieurs relevés successifs
**afin de** pouvoir valider un mois même quand ses dernières opérations n'apparaissent que sur le relevé du mois suivant (les cycles bancaires ne suivent pas le calendrier), sans avoir à me souvenir d'une astuce d'usage.

**Contexte :** Le contrôle actuel ("Contrôler avec un relevé") est entièrement éphémère : chaque clic recalcule tout à partir de zéro et **remplace** le résultat précédent, sans rien mémoriser. Or un mois donné peut légitimement avoir des opérations de fin de mois qui n'apparaissent que sur le relevé du mois suivant (ex. un paiement carte du 31/07 encaissé le 03/08, donc visible seulement sur le relevé « suivant »). Avec un seul relevé à la fois, impossible d'obtenir zéro écart pour un tel mois — et « Valider ce mois » exige zéro écart. Découvert en creusant une question du PO sur ce point précis.

**Bonne surprise en creusant le code existant :** le modèle a déjà tout ce qu'il faut, jamais branché à l'écran :
- `Achat.Rapproche` (bool, persisté) et `Achat.DatePrelevement`/`LibelleReleve` (remplis au rapprochement) existent déjà dans `Achat.cs`.
- `ReconciliationService.Rapprocher` exclut déjà les achats `Rapproche == true` de la recherche de candidats (`achatsCourant.Where(a => !a.Rapproche)`) — donc un deuxième contrôle avec un autre relevé ne recherche déjà, mécaniquement, que les achats pas encore rapprochés.
- `AchatsRepository.AppliquerRapprochements(annee, matches)` existe déjà, complet, prêt à marquer `Rapproche=true` et à compléter `DatePrelevement`/`RaisonSociale`/`LibelleReleve` à partir des matches — **mais n'est appelée nulle part**.

**Workflow cible (proposé par le PO) :**
1. Je contrôle avec le premier relevé — tout ce qui correspond est marqué rapproché (persisté).
2. Je contrôle avec un deuxième relevé (même mois ou mois suivant) — seuls les achats encore non rapprochés sont recherchés, ce qui marque le reste.
3. Dès que tous les achats du mois sont rapprochés (peu importe en combien de contrôles), « Valider ce mois » devient possible.
4. Un moyen de réinitialiser le contrôle d'un mois en cas d'erreur (redémarrer à zéro le rapprochement, sans perdre les achats eux-mêmes).

**Pistes à trancher à la reprise :**
- Faut-il persister automatiquement à chaque clic sur « Contrôler », ou ajouter une étape de confirmation avant d'écrire (« Confirmer ces N rapprochements ») ? Le PO privilégie l'intuitivité — probablement automatique, à valider.
- Redéfinir la condition de `PeutValider` : ne plus se baser sur le dernier résultat éphémère (`ControleSansEcart`), mais sur l'état persisté (tous les achats courant/dépenses du mois ont `Rapproche == true`, et aucune ligne du dernier relevé importé n'est restée "à saisir" pour ce mois).
- Action de réinitialisation : remettre `Rapproche=false` (et vider `DatePrelevement`/`LibelleReleve` complétés automatiquement ?) pour les achats d'un mois donné — portée exacte à définir (un mois entier ? un achat à la fois ?).
- Revoir la documentation du code qui décrit le contrôle comme « éphémère » (commentaire en tête de `MoisViewModel.cs` notamment) une fois ce changement fait.

**Conditions de réussite (à affiner à la reprise) :**
- [ ] Décision PO sur la confirmation automatique vs manuelle avant persistance.
- [ ] `AppliquerRapprochements` appelée depuis `Controler()`/`AnalyserControle()`.
- [ ] `PeutValider` basé sur l'état persisté, plus plusieurs relevés successifs suffisent à couvrir un mois à cheval sur deux cycles bancaires.
- [ ] Action de réinitialisation du contrôle d'un mois, testée en cas d'erreur.
- [ ] Testé par le PO sur un cas réel à cheval sur deux relevés (ex. juillet 2026).

---

### US-006 — Ouvrir le sélecteur de fichier sur le dossier des relevés
**En tant qu'**utilisateur, **je veux** que le sélecteur de fichier de « Contrôler avec un relevé » s'ouvre directement sur le dossier où atterrissent mes xlsx extraits
**afin de** ne pas avoir à naviguer manuellement jusqu'au bon dossier à chaque import mensuel.

**Contexte :** Bug remonté par le PO : le sélecteur pointait par défaut sur un chemin de développement (repo `sample-data\`), pas pertinent en usage réel — chaque utilisateur range ses relevés où il veut, impossible de deviner un chemin figé.

**Décisions PO (livrées) :**
- Dossier par défaut au tout premier usage : dossier Documents de l'utilisateur (`Environment.SpecialFolder.MyDocuments`).
- Ensuite, mémorisation automatique du dernier dossier utilisé (`PreferencesRepository`, nouveau `preferences.json`, même esprit que `fenetre.json` pour la fenêtre) — pas de champ de configuration séparé à remplir : le PO « définit son emplacement » simplement en y naviguant une première fois (ex. `D:\Alejandro\Mes Documents\Fichiers de travail\Administration\Banque\Relevés`), l'appli s'en souvient ensuite.
- `MoisViewModel` reçoit désormais `PreferencesRepository` en plus des autres repositories (fil de construction mis à jour : `App.xaml.cs` → `MainWindowViewModel` → `MoisViewModel`).

**Conditions de réussite :**
- [x] Décision PO sur le dossier par défaut (Documents) et la mémorisation (dernier dossier utilisé).
- [x] Le sélecteur s'ouvre sur ce dossier au lieu du dossier par défaut de Windows.
- [ ] Testé par le PO dans l'appli (le dossier choisi la fois précédente est bien réutilisé).

---

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

## Terminé

### US-007 — Aide utilisateur accessible depuis l'application
**En tant qu'**utilisateur, **je veux** accéder à une aide directement depuis l'application (icône « ? » à droite de l'onglet Admin dans la barre d'onglets)
**afin de** ne pas devoir chercher et ouvrir `README.txt` séparément quand j'ai un doute sur le fonctionnement d'un écran.

**Décisions PO (livrées) :**
- Mode d'affichage : ouverture de `README.txt`/`README-utilisateur.txt` avec l'application par défaut du système (`Process.Start`), pas de fenêtre interne — plus simple, réutilise le mode d'emploi déjà existant tel quel.
- `CheminAideResolver` (Data, `Paths/CheminAideResolver.cs`) résout le bon fichier selon le contexte : `README.txt` à côté de l'exécutable en publié, `README-utilisateur.txt` à la racine du dépôt en `dotnet run` (réutilise `DataPathProvider.TrouverRacineDepot`, rendue publique à cet effet).
- Icône « ? » ronde (`StyleBoutonAide`) ajoutée à droite du groupe Budget/Admin dans `MainWindow.xaml`, distincte visuellement des onglets de navigation.
- Modale d'erreur générique (`ConfirmationModalViewModel`) si le fichier d'aide est introuvable.

**Conditions de réussite :**
- [x] Décision PO sur le mode d'affichage.
- [x] Icône « ? » visible à droite de l'onglet Admin, accessible en un clic depuis n'importe quel écran.
- [x] Testé par le PO dans l'appli : clic sur « ? » ouvre bien le mode d'emploi.

**Livré le :** 2026-09-25.

### US-005 — Importer un relevé bancaire via extraction Claude (PDF → xlsx)
**En tant qu'**utilisateur, **je veux** importer mensuellement un relevé bancaire xlsx généré par Claude à partir du PDF CIC (export CSV bancaire indisponible, voir US-003)
**afin de** pouvoir alimenter le contrôle de rapprochement avec de vraies données, sans dépendre de l'export CSV du site CIC.

**Décisions PO (livrées) :**
- Le Livret DDS ne bouge que par virement vers/depuis le compte courant (pas de virement direct, intérêts négligeables) — déjà visible côté compte courant, donc seul cet onglet est lu. Pas de règle de mapping Épargne à construire : les virements s'étiquettent avec les postes Épargne (dépôt)/(retrait) comme n'importe quelle autre ligne importée.
- Étend le contrôle éphémère existant ("Contrôler avec un relevé") plutôt que de construire l'écran Import persisté à 3 blocs prévu à l'origine dans PROMPT.md — hors besoin réel actuel.
- `ReleveXlsxParser` (Domain, `Csv/ReleveXlsxParser.cs`) + `XlsxReleveFileReader` (Data, `Xlsx/XlsxReleveFileReader.cs`, ClosedXML) produisent la même forme `LigneReleve` que `CicCsvParser` — `ReconciliationService` et le contrôle éphémère n'ont pas changé.
- Réutilisation de l'heuristique `ExtraireRaisonSociale` existante, extraite dans un helper partagé `LibelleReleveHeuristiques`. Affinée pour le xlsx : pour un paiement carte/retrait, la raison sociale vient de la colonne "Détail" (évite de dupliquer le nom de ville présent dans "Opération", défaut découvert en testant sur données réelles) ; pour un prélèvement/virement, elle vient de "Opération" comme avant.
- Prompt Claude figé et versionné dans `docs/prompt-extraction-releve-cic.md`.
- Bouton "Contrôler avec un relevé" (`MoisViewModel.Controler()`) accepte désormais `.csv` et `.xlsx`, aiguillé sur l'extension du fichier choisi.
- US-003 remplacée par cette approche (décision PO) — voir plus bas.

**Conditions de réussite :**
- [x] Le prompt Claude utilisé pour produire l'extraction est figé/versionné (`docs/prompt-extraction-releve-cic.md`).
- [x] Règle de mapping Épargne : pas nécessaire (voir décisions PO ci-dessus).
- [x] Parseur xlsx écrit et testé (12 tests Domain + 2 tests Data + vérification manuelle sur le relevé réel du PO : 62 lignes brutes → 59 transactions, solde net -256,48€ conforme au PDF, aucune raison sociale vide).
- [x] Test de bout en bout par le PO dans l'application (bouton "Contrôler avec un relevé" avec le vrai xlsx d'août 2026) : 7 orphelines, 10 lignes "à saisir" — cohérent avec la vérification manuelle en amont.
- [x] Décision PO sur le sort de US-003 : remplacée par cette approche.

**Livré le :** 2026-09-25.

### US-003 — Valider le contrôle de rapprochement en conditions réelles
**En tant qu'**utilisateur, **je veux** tester le contrôle de rapprochement bancaire avec un vrai relevé de ma banque
**afin de** m'assurer qu'il fonctionne correctement sur mes propres relevés, pas seulement sur des fixtures synthétiques.

**Décision PO (livrée) :** l'export CSV officiel du site CIC reste inaccessible, mais l'objectif de cette US (valider le rapprochement en conditions réelles) est atteint via US-005 (extraction PDF → xlsx par Claude). Remplacée par US-005, pas de suite à donner sur l'export CSV du site tant qu'il n'est pas redemandé explicitement.

**Conditions de réussite :**
- [x] Le contrôle de rapprochement a été testé sur un vrai relevé (via US-005) et donne des résultats cohérents, validés par le PO.

**Livré le :** 2026-09-25 (via US-005).

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
