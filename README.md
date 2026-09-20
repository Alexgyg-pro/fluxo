# alxBudgetDesktop

Portage en application de bureau WPF/.NET du projet [alxBudget](../alxBudget)
(gestion de budget personnel). Voir `PROMPT.md` pour le contexte complet et
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

**Ne jamais pointer l'application sur le dossier `data\` réel d'alxBudget**
avant validation explicite à l'usage — il contient de vraies données
financières en production.
