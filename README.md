# Fluxo

Application de bureau Windows (WPF/.NET 8) de gestion de budget personnel.

Fluxo s'appelait à l'origine **alxBudgetDesktop** : le port desktop d'alxBudget,
une application web (Node/Express + React/TypeScript) plus ancienne du même
auteur, pour le même besoin — d'où certains noms internes (projets,
namespaces, dossier de données) qui gardent encore cette ancienne
appellation. Voir `PROMPT.md` pour le contexte complet du portage et
`AIPLAN.md` (non commité) pour le suivi de l'avancement.

## Lancer l'application

```powershell
dotnet run --project src\AlxBudgetDesktop.App\AlxBudgetDesktop.App.csproj
```

## Lancer les tests du domaine métier

```powershell
dotnet test tests\AlxBudgetDesktop.Domain.Tests\AlxBudgetDesktop.Domain.Tests.csproj
```

## Données

Par défaut, l'application lit/écrit dans un dossier `sample-data\` (données
synthétiques, aucun risque). Le dossier utilisé peut être changé via la
variable d'environnement `ALXBUDGET_DESKTOP_DATA_DIR`.

**Ne jamais pointer l'application sur un dossier de données réelles** avant
validation explicite à l'usage.

## Développeur

Créer la version de prod dans `publish\` :

```powershell
dotnet publish src\AlxBudgetDesktop.App\AlxBudgetDesktop.App.csproj -r win-x64 --self-contained true -p:PublishSingleFile=true -o publish
```

Copier `publish\` vers Programmes (x86) (terminal en administrateur — clic
droit → "Exécuter en tant qu'administrateur", et vérifier que l'appli est
fermée avant). Le dossier d'installation existant s'appelle encore
`alxBudgetDesktop\` (nom hérité, jamais renommé — voir note ci-dessus) :

```powershell
xcopy "publish\*" "C:\Program Files (x86)\alxBudgetDesktop\" /Y /E
```
