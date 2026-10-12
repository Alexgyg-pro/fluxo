# Contrôle bancaire

Le bouton **Contrôler avec un relevé** compare vos saisies à un relevé bancaire pour repérer ce qui manque des deux côtés.

## Formats acceptés

- Un export CSV téléchargé depuis le site de votre banque.
- Un fichier xlsx, par exemple si vous faites extraire un relevé PDF par un assistant IA (voir le thème **Extraire un relevé (prompt)** dans cette aide pour un prompt reproductible).

## Ce que montre le contrôle

- **à saisir** : une opération présente sur le relevé mais jamais saisie dans l'appli. Cliquez dessus pour l'ajouter, champs déjà pré-remplis.
- **orpheline** : l'inverse — une opération saisie chez vous introuvable sur le relevé importé. À vérifier : montant ou date incorrects, ou peut-être une dépense payée autrement qu'avec ce compte (liquide, par exemple).

## Le rapprochement est mémorisé

Un rapprochement réussi est **définitif** : vous pouvez contrôler un même mois plusieurs fois, avec plusieurs relevés successifs, sans jamais perdre ce qui a déjà été validé — seul ce qui reste non rapproché est recherché à chaque nouveau contrôle. En cas d'erreur (mauvais fichier importé par mégarde...), le bouton **Réinitialiser le contrôle** remet le mois à zéro sans supprimer vos opérations.

## Deux pièges à connaître (relevés CIC, probablement valables ailleurs)

**Le nommage du relevé ne correspond pas au mois qu'il couvre.** Un relevé est daté du jour où la banque l'a édité, pas du mois qu'il couvre réellement : un fichier "au 01/08" contient l'essentiel de **juillet**, pas août. Une carte utilisée le 31/07 peut n'être débitée que le 03/08 — cette opération n'apparaîtra alors que sur le relevé **suivant**. Conséquence pratique : un mois demande souvent 2 relevés successifs (parfois 3) pour arriver à zéro écart. Ce n'est pas une erreur si "Valider ce mois" n'apparaît pas tout de suite.

**Les petits paiements sont parfois regroupés.** Certaines banques regroupent automatiquement plusieurs paiements de moins de 10€ au même commerçant le même jour en une seule ligne de relevé (ex. deux achats de 6,25€ et 7,22€ deviennent une ligne "2 PAIEMENTS" de 13,47€). L'appli ne sait rapprocher qu'une opération pour une ligne de relevé — si ça vous arrive, fusionnez vos deux saisies séparées en une seule du même montant total pour qu'elle se rapproche.
