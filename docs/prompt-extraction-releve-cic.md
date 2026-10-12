# Prompt d'extraction du relevé bancaire CIC (PDF → xlsx)

À utiliser chaque mois avec Claude (claude.ai), en joignant le PDF du relevé
CIC téléchargé depuis l'espace client. Le prompt est figé pour garantir une
structure stable d'un mois sur l'autre — l'import xlsx de Fluxo attend
exactement cette forme (voir `Fluxo.Data.Xlsx.XlsxReleveFileReader`
et `Fluxo.Domain.Csv.ReleveXlsxParser`).

**Ne pas improviser une variante du prompt sans mettre à jour ce fichier ET
vérifier que l'import fonctionne toujours** (le nom de l'onglet et les
en-têtes de colonnes sont comparés tels quels, insensibles à la casse/aux
espaces mais pas au reste).

**Ce fichier est la source de vérité technique** (références aux classes
C#, destiné aux sessions de développement). Une copie orientée utilisateur
existe dans l'aide en ligne de l'app (`src/Fluxo.App/Aide/ExtraireUnReleve.md`,
thème "Extraire un relevé (prompt)") — sans les références de code, pour
rester lisible hors contexte de dev. Toute modification du prompt ou de la
checklist de validation doit être répercutée manuellement dans les deux.

---

## Prompt à copier-coller

```
Voici le PDF de mon relevé bancaire CIC du mois. Extrais-en un fichier xlsx
avec la structure suivante, sans rien ajouter ni interpréter au-delà de ce
qui est écrit dans le PDF :

- Un onglet nommé exactement "Compte courant" pour le compte courant
  (Service accueil), avec ces colonnes dans cet ordre exact :
  Date | Date valeur | Opération | Détail | Débit (EUR) | Crédit (EUR)

  - "Date" et "Date valeur" au format texte ISO AAAA-MM-JJ (pas de date
    Excel, pas de format JJ/MM/AAAA).
  - "Opération" : le libellé principal de l'opération tel qu'il apparaît sur
    le relevé (ex. "PAIEMENT PSC 0108 TRAPPES", "PRLV SEPA LES RESIDENCES").
  - "Détail" : la ligne secondaire quand elle existe (ex. nom du commerçant
    "MARKET TRAPPES CARTE 5299", ou la référence de prélèvement
    "LOYER:202607") — vide sinon.
  - "Débit (EUR)" et "Crédit (EUR)" : montants positifs, un seul des deux
    renseigné par ligne.
  - Une première ligne "SOLDE CRÉDITEUR (ouverture)" avec le solde de début
    de période en colonne Crédit.
  - Une ligne "TOTAL DES MOUVEMENTS" avec la somme des débits et des crédits.
  - Une dernière ligne "SOLDE CRÉDITEUR (clôture)" avec le solde de fin de
    période en colonne Crédit.

- Un onglet "Livret DDS" avec la même structure pour le Livret de
  Développement Durable Solidaire, si présent dans le PDF.

- Un onglet "Résumé" avec le titulaire, la période, et pour chaque compte
  son solde d'ouverture et de clôture.

Vérifie que la somme (solde d'ouverture - débits + crédits) redonne bien le
solde de clôture indiqué sur le PDF pour chaque compte, avant de me
communiquer le fichier.
```

## Pourquoi ces contraintes précises

- **Nom d'onglet exact "Compte courant"** : c'est le seul onglet lu par
  l'import (le Livret DDS n'apporte rien à l'application — ses seuls
  mouvements sont des virements déjà visibles côté compte courant).
- **Dates en texte ISO** : évite toute conversion de locale ; c'est déjà le
  format interne de l'application.
- **Opération / Détail séparés** : permet d'extraire une raison sociale
  propre sans dupliquer le nom de ville qui suit parfois le code opération
  (ex. "PAIEMENT PSC 0108 TRAPPES" + "MARKET TRAPPES CARTE 5299" →
  "MARKET TRAPPES", pas "TRAPPES MARKET TRAPPES").
- **Lignes de synthèse (ouverture/clôture/total)** : reconnues et ignorées
  par le parseur via leur libellé (normalisé, insensible aux accents/casse) —
  ne pas s'inquiéter si Claude les orthographie légèrement différemment d'un
  mois sur l'autre, mais éviter de les supprimer : elles servent à vérifier
  soi-même la cohérence du fichier avant import.

## Validation avant import dans l'appli

Avant d'importer un nouveau fichier, vérifier à l'œil que :
- Le solde d'ouverture du mois correspond au solde de clôture du mois
  précédent.
- Le solde de clôture recalculé (ouverture - débits + crédits) correspond
  bien à celui indiqué sur le PDF.
