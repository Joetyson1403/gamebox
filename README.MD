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
