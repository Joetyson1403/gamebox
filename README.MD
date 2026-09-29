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

## GBX-71 - Authentification

Le site propose une création de compte et une connexion avec authentification par cookie ASP.NET Core.

### Fonctionnalités

- Création d'un compte membre avec username, email et mot de passe.
- Vérification de l'email, de la longueur du username et de la confirmation du mot de passe.
- Vérification de l'unicité du username et de l'email.
- Hachage des mots de passe avec `BCrypt.Net-Next` avant l'enregistrement en base.
- Connexion avec le username ou l'email.
- Cookie de session persistant pendant 14 jours avec expiration glissante.
- Option « Keep me signed in » pour conserver la session.
- Déconnexion avec invalidation du cookie.
- Protection antiforgery sur les formulaires POST.
- Logo Gamebox affiché depuis `wwwroot/images/gamebox_logo.png`.

### Routes d'authentification

| Méthode | URL | Description |
| --- | --- | --- |
| `GET` | `/Auth/Login` | Affiche le formulaire de connexion |
| `POST` | `/Auth/Login` | Vérifie les identifiants et crée le cookie |
| `GET` | `/Auth/Register` | Affiche le formulaire d'inscription |
| `POST` | `/Auth/Register` | Crée le membre et ouvre sa session |
| `POST` | `/Auth/Logout` | Supprime le cookie de session |

### Lancer l'application

Depuis le dossier du projet :

```powershell
dotnet restore
dotnet run
```

Le profil de lancement ouvre automatiquement la page de connexion :

- HTTP : `http://localhost:5231/Auth/Login`
- HTTPS : `https://localhost:7035/Auth/Login`

### Compte de démonstration local

Un compte de démonstration a été créé dans la base SQLite locale `gamebox.db` :

| Champ | Valeur |
| --- | --- |
| Username | `demo` |
| Email | `demo@gamebox.local` |
| Mot de passe | `Gamebox123!` |

Ce compte est prévu uniquement pour le développement local. Il ne doit pas être utilisé en production.

### Tests GBX-71

#### Tests manuels fonctionnels

1. Ouvrir `/Auth/Register` et vérifier que le formulaire contient username, email, mot de passe et confirmation.
2. Soumettre un formulaire valide et vérifier la redirection vers `/`.
3. Vérifier que la barre de navigation affiche `Hello, demo` après l'inscription.
4. Se déconnecter, ouvrir `/Auth/Login`, puis se connecter avec le username `demo`.
5. Refaire le test de connexion avec l'email `demo@gamebox.local`.
6. Vérifier qu'un mauvais mot de passe affiche une erreur sans créer de session.
7. Vérifier qu'un username ou un email déjà utilisé est refusé à l'inscription.
8. Vérifier que la déconnexion retire les informations de session et renvoie vers `/Auth/Login`.
9. Fermer puis rouvrir le navigateur avec l'option `Keep me signed in` activée et vérifier la persistance du cookie.

#### Tests techniques exécutés

```powershell
dotnet build
```

Le build doit se terminer sans erreur.

Test rapide des routes avec PowerShell :

```powershell
$login = Invoke-WebRequest http://localhost:5231/Auth/Login -UseBasicParsing
$register = Invoke-WebRequest http://localhost:5231/Auth/Register -UseBasicParsing

$login.StatusCode
$register.StatusCode
```

Les deux réponses attendues sont `200`. Le projet ne contient pas encore de projet de tests automatisés xUnit/NUnit ; les tests fonctionnels ci-dessus sont donc des tests d'intégration manuels exécutables contre l'application locale.
