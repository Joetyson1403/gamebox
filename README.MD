# GameBox

## Description du projet
Gamebox est une plateforme web de suivi et de critique de jeux-vidéos s’inspirant du modèle de Letterboxd. Gamebox met à disposition un catalogue interactif où les utilisateurs peuvent explorer des titres, attribuer des notes (sur 5 étoiles), rédiger des avis et consulter la moyenne des évaluations attribuées par la communauté. Chaque membre peut alimenter son journal de bord en consignant les jeux terminés, en cours ou à jouer.

## Stack Technique
- **Back-end :** C# ASP.NET Core MVC 
- **Base de données :** SQLite avec Entity Framework Core (Code-First)
- **Front-end :** HTML, CSS, framework Bootstrap 5
- **Données externes :** API RAWG

## Architecture Technique (POO)
Le projet respecte les concepts de la programmation orientée objet, avec notamment une classe abstraite `User` héritée par `Member` et `Administrator`.

## GBX-6 - Fiche jeu
La fonctionnalité de fiche jeu permet de cliquer sur un jeu depuis le catalogue et d'afficher une page de détails.

La fiche contient :
- la couverture et le titre du jeu ;
- le développeur et la date de sortie ;
- le synopsis, limité à cinq lignes ;
- les genres sous forme de tags ;
- la note moyenne et le nombre d'avis de la communauté.

Les informations générales sont récupérées depuis l'API RAWG. La note communautaire est calculée à partir des avis enregistrés dans la base SQLite.

Pour utiliser cette fonctionnalité, renseigner une clé RAWG valide dans `appsettings.json`, puis lancer le projet avec :

```powershell
dotnet run
```

### Tests GBX-6
- Vérifier que les cartes du catalogue redirigent vers `/Games/Details/{id}`.
- Vérifier que le titre, la couverture, le développeur et le synopsis correspondent au jeu sélectionné.
- Vérifier que le synopsis ne dépasse pas cinq lignes.
- Vérifier que les genres RAWG apparaissent sous forme de tags.
- Vérifier que la note communautaire et le nombre d'avis sont affichés lorsque des avis existent.
- Vérifier qu'une erreur claire apparaît si l'API RAWG est inaccessible.
- Validation technique effectuée avec `dotnet build`.
