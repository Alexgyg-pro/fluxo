# Extraire un relevé bancaire (prompt PDF → xlsx)

Si votre banque ne propose plus d'export CSV directement utilisable (voir **Contrôle bancaire**), vous pouvez faire extraire le relevé PDF au format xlsx par un assistant IA (Claude, par exemple), avec le prompt ci-dessous.

Le prompt est volontairement figé : la structure qu'il décrit est exactement celle que l'appli sait lire. Ne l'improvisez pas d'un mois sur l'autre.

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

- **Nom d'onglet exact "Compte courant"** : c'est le seul onglet lu par l'import (le Livret DDS n'apporte rien à l'appli — ses seuls mouvements sont des virements déjà visibles côté compte courant).
- **Dates en texte ISO** : évite toute conversion de locale.
- **Opération / Détail séparés** : permet d'extraire une raison sociale propre sans dupliquer le nom de ville qui suit parfois le code opération (ex. "PAIEMENT PSC 0108 TRAPPES" + "MARKET TRAPPES CARTE 5299" → "MARKET TRAPPES", pas "TRAPPES MARKET TRAPPES").
- **Lignes de synthèse (ouverture/clôture/total)** : reconnues et ignorées par l'import grâce à leur libellé, même si Claude les orthographie légèrement différemment d'un mois sur l'autre — mais évitez de les supprimer : elles servent à vérifier vous-même la cohérence du fichier avant import.

## Validation avant import dans l'appli

Avant d'importer un nouveau fichier, vérifiez à l'œil que :
- Le solde d'ouverture du mois correspond au solde de clôture du mois précédent.
- Le solde de clôture recalculé (ouverture - débits + crédits) correspond bien à celui indiqué sur le PDF.
