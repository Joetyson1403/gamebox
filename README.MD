# GameBox

## Description du projet
Gamebox est une plateforme web de suivi et de critique de jeux-vidéos s’inspirant du modèle de Letterboxd. Gamebox met à disposition un catalogue interactif où les utilisateurs peuvent explorer des titres, attribuer des notes (sur 5 étoiles), rédiger des avis et consulter la moyenne des évaluations attribuées par la communauté. Chaque membre peut alimenter son journal de bord en consignant les jeux terminés, en cours ou à jouer.

## Stack Technique
- **Back-end :** C# ASP.NET Core MVC 
- **Base de données :** SQLite avec Entity Framework Core (Code-First)
- **Front-end :** HTML, CSS, framework Bootstrap 5
- **Données externes :** API RAWG

---

## Architecture des Données : Base SQLite & API Externe

Le projet repose sur une **architecture hybride** combinant une API REST externe et une base de données relationnelle locale :

```text
[ API RAWG ] (Catalogue mondial de +500'000 jeux)
     │
     ▼ (Recherche & Consultation)
[ Contrôleurs ASP.NET Core ] ── (Synchronisation & Logique métier)
     │
     ▼ (Entity Framework Core)
[ Base SQLite : gamebox.db ] (Données propres à la plateforme)
```

### 1. Pourquoi combiner API et Base de Données ?
- **L'API externe (RAWG) :** Fournit un catalogue mondial dynamique sans avoir à saisir manuellement des milliers de jeux, jaquettes, studios et descriptions.
- **La Base locale (`gamebox.db`) :** Permet la persistance des données propres aux utilisateurs de Gamebox. Comme il est impossible d'écrire sur les serveurs distants de RAWG, toutes les actions des utilisateurs (comptes, critiques, notes, statuts, listes) sont sauvegardées dans notre base SQLite.
- **Synchronisation automatique :** Dès qu'un utilisateur note un jeu ou modifie son statut, le contrôleur vérifie si le jeu existe déjà dans la table locale `Games`. Si ce n'est pas le cas, il y est automatiquement importé et enregistré.

### 2. Rôle des Tables SQLite
L'application s'appuie sur **7 tables métier** et des tables techniques générées par Entity Framework Core :

| Table | Description | Clés / Relations |
| :--- | :--- | :--- |
| **`Games`** | Catalogue local des jeux importés depuis RAWG (Titre, Studio, Année, Jaquette, Note moyenne calculée). | Clé primaire `Id`. |
| **`Users`** | Comptes utilisateurs. Applique le pattern **TPH (Table Per Hierarchy)** : regroupe `Member` et `Administrator` avec le discriminateur `UserType`. | Clé primaire `Id`. |
| **`Reviews`** | Avis textuels et notes (1 à 5 étoiles) rédigés par les membres. | Clé étrangère vers `Users(Id)` et `Games(Id)`. |
| **`MemberGameStatuses`** | Suivi du backlog d'un joueur (*Wishlist*, *In Progress*, *Completed*, etc.). | Clé composite `(MemberId, GameId)` garantissant 1 seul statut par jeu/joueur. |
| **`CustomLists`** | Listes de jeux personnalisées créées par un membre (ex: "Mes coups de cœur 2024"). | Clé étrangère vers `Users(Id)`. |
| **`CustomListGame`** | Table de jointure (relation N:N) associant plusieurs jeux à plusieurs listes. | Clés composites `(CustomListsId, GamesInListId)`. |
| **`MemberFavoriteGames`** | Table de liaison (relation N:N) pour les jeux mis en favoris par un membre. | Clés composites `(FavoritedById, FavoriteGamesId)`. |
| **`__EFMigrationsHistory`** | Table technique d'EF Core assurant le suivi de versioning du schéma SQL. | Clé primaire `MigrationId`. |

### 3. Visualisation de la base (`gamebox.db`)
Le fichier de base de données se trouve à la racine du projet (`gamebox.db`). Il peut être inspecté :
- **Dans l'IDE :** Via l'extension **SQLite Viewer** en cliquant directement sur le fichier `gamebox.db`.
- **Avec un outil dédié :** Via le logiciel gratuit **DB Browser for SQLite** (onglets *Structure de la base de données* et *Parcourir les données*).

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

### GBX-9 - Tableau de bord personnel
Accéder à un profil utilisateur pour voir ses statistiques, ses jeux favoris, ses listes personnalisées et sa collection classée par statut.
- **Code (Modèles) :** Création des classes `CustomList` et `CustomListGame` (table de liaison) pour gérer les listes de jeux par utilisateur. Ajout de propriétés de navigation dans le modèle `Member` pour relier le profil à ses favoris, ses listes et ses statuts de jeux.
- **Contrôleur :** Création de `ProfileController` avec des actions dédiées (`Index`, `Edit`, `Collection`, `ListDetails`) permettant de récupérer et manipuler les données relatives au profil (statistiques, création/suppression de listes, ajout aux favoris, édition de l'avatar et de la bio).
- **Vue & UI :** Création d'un tableau de bord moderne et responsive en Bootstrap. Intégration de sections dynamiques (compteurs de jeux calculés en temps réel, affichage sous forme de cartes pour la collection et les listes) et de menus contextuels pour ajouter facilement des jeux aux listes depuis la fiche du jeu.

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
