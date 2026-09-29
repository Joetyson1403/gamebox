# GameBox

## Description du projet
Gamebox est une plateforme web de suivi et de critique de jeux-vidéos s’inspirant du modèle de Letterboxd. Gamebox met à disposition un catalogue interactif où les utilisateurs peuvent explorer des titres, attribuer des notes (sur 5 étoiles), rédiger des avis et consulter la moyenne des évaluations attribuées par la communauté. Chaque membre peut alimenter son journal de bord en consignant les jeux terminés, en cours ou à jouer.

## Stack Technique
- **Back-end :** C# ASP.NET Core MVC 
- **Base de données :** SQLite avec Entity Framework Core (Code-First)
- **Front-end :** HTML, CSS, framework Bootstrap 5
- **Données externes :** API RAWG

---

## Fonctionnalités du Sprint & Détails d'Implémentation

### GBX-65 - Création des modèles POO et génération des tables SQLite
Mise en place de l'architecture de la base de données.
- **Modélisation (POO) :** Création des classes C# dans le dossier `Models/` (`User`, `Member`, `Administrator`, `Game`, `Review`, `CustomList`). Utilisation de l'héritage pour différencier les rôles au sein d'une seule table `Users`.
- **EF Core :** Configuration du contexte `AppDbContext` avec `DbSet` et utilisation de l'API Fluente (`OnModelCreating`) pour définir les clés primaires composites et les relations entre les tables.
- **Migrations :** Génération du schéma de la base de données locale `gamebox.db` via les commandes `dotnet ef migrations add` et `dotnet ef database update`.

### GBX-71 - Système d'authentification (Inscription / Connexion)
Création de compte et connexion avec authentification par cookie ASP.NET Core.
- **Modèles :** Hachage sécurisé des mots de passe avec la librairie `BCrypt.Net-Next` avant l'enregistrement en base.
- **Contrôleur (`AuthController`) :** Traitement des vues d'inscription et de connexion. Création d'un cookie de session (`HttpContext.SignInAsync`) persistant pendant 14 jours (avec option « Keep me signed in »). Gestion de la déconnexion avec invalidation du cookie et vérification stricte de l'unicité du nom d'utilisateur et de l'email.
- **Vues :** Formulaires sécurisés avec protection antiforgery, champs requis, et affichage dynamique du statut connecté (Hello, Username) dans la barre de navigation.

### GBX-5 - Recherche de jeux
Implémentation d'une barre de recherche connectée à l'API RAWG pour parcourir le catalogue.
- **Code Service :** Création de l'interface `IGameApiService` et de son implémentation `GameApiService` utilisant un `HttpClient` pour requêter l'API externe avec une clé configurée dans `appsettings.json`.
- **Contrôleur :** L'action `Search` du `GamesController` traite la requête GET, interroge l'API, et retourne les résultats via une liste d'objets DTO (`GameDto`).
- **Vue :** Affichage asynchrone des résultats sous forme de cartes fluides intégrées dans une grille Bootstrap.

### GBX-6 - Fiche détaillée d'un jeu
Consultation d'une fiche complète lors du clic sur un jeu du catalogue.
- **Code Service :** Ajout de la méthode `GetGameDetailsAsync(id)` dans `GameApiService` pour interroger l'endpoint spécifique d'un jeu (API RAWG) et désérialiser la réponse JSON (`RawgGameDetailsDto`).
- **Contrôleur :** L'action `Details(int id)` fusionne les données externes (développeur, couverture, synopsis fournis par l'API) et les données internes (liste des avis communautaires `Review` récupérés via SQLite pour le calcul de la moyenne).
- **Vue (`Details.cshtml`) :** Utilisation de Razor (C#) pour extraire et limiter dynamiquement la taille du synopsis. Intégration de l'image de couverture (`BackgroundImage`) en en-tête de page.

### GBX-7 - Rédiger une critique et noter
Permettre à un joueur d'attribuer une note sur 5 et de rédiger un avis public.
- **Code (Modèles) :** La classe `Review` lie un utilisateur (`MemberId`), un jeu (`GameId`), et stocke une note (`Rating`) ainsi qu'un commentaire (`Comment`).
- **Contrôleur :** L'action HTTP POST `AddReview` s'assure via l'attribut `[Authorize]` que l'utilisateur est connecté. Elle garantit l'existence locale du jeu, insère la critique en base, puis recalcule et met à jour dynamiquement la note communautaire moyenne (`AverageRating`) du jeu.
- **Vue :** Affichage d'une fenêtre modale Bootstrap permettant la soumission du formulaire, et apparition immédiate de l'avis dans la liste chronologique sous la fiche du jeu.

### GBX-8 - Définir le statut d'un jeu
Classer un jeu (À jouer, En cours, Terminé, Abandonné) sans rafraîchir la page Web.
- **Code (Modèles) :** Création de l'énumération `GameStatus` et de la table de liaison `MemberGameStatus` pour sauvegarder l'état personnel d'un jeu pour chaque compte.
- **Contrôleur :** L'action POST `UpdateGameStatus` vérifie la présence du header HTTP `X-Requested-With` pour identifier une requête JavaScript, renvoyant ainsi une réponse JSON (`{ success: true }`) au lieu d'une redirection HTML classique.
- **Vue & JavaScript :** Utilisation d'un composant de type menu déroulant (dropdown) Bootstrap. Un script JavaScript intercepte l'événement de soumission, contacte le serveur en tâche de fond avec l'API `fetch`, et met à jour en temps réel l'apparence visuelle du bouton (couleur CSS et libellé) selon le nouveau statut choisi.

---

## Instructions d'exécution
Depuis le dossier racine du projet (contenant le fichier `.csproj`), exécutez :
```powershell
dotnet restore
dotnet build
dotnet run
```
L'application sera accessible localement via les ports indiqués (ex: `http://localhost:5231` ou `https://localhost:7035`).
*(Rappel : Pour accéder aux données du catalogue, veillez à utiliser une clé API RAWG valide).*
