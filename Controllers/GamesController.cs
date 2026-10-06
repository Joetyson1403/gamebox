using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using gamebox.Data;
using gamebox.Models;
using gamebox.Services;
using Microsoft.AspNetCore.Authorization;

namespace gamebox.Controllers;

/// <summary>
/// Contrôleur principal pour la gestion des jeux : recherche, consultation de fiche,
/// gestion des critiques (ajout/modification/suppression) et suivi du statut de jeu (backlog).
/// </summary>
public class GamesController : Controller
{
    private readonly IGameApiService _gameApiService;
    private readonly AppDbContext _dbContext;

    public GamesController(IGameApiService gameApiService, AppDbContext dbContext)
    {
        _gameApiService = gameApiService;
        _dbContext = dbContext;
    }

    /// <summary>
    /// Action GET : Recherche des jeux dans le catalogue mondial via l'API RAWG.
    /// Si la requête est vide, affiche les jeux populaires du moment.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Search(string? query)
    {
        ViewBag.Query = query;

        try
        {
            // Appel au service externe RAWG
            var games = await _gameApiService.SearchGamesAsync(query ?? string.Empty);

            if (games == null || !games.Any())
            {
                ViewBag.ErrorMessage = "Aucun résultat trouvé pour votre recherche.";
                return View(new List<GameDto>());
            }

            return View(games);
        }
        catch (Exception ex) when (ex.Message == "Timeout")
        {
            ViewBag.ErrorMessage = "La requête vers l'API a mis trop de temps à répondre (Timeout). Veuillez réessayer.";
            return View(new List<GameDto>());
        }
        catch (Exception)
        {
            ViewBag.ErrorMessage = "Une erreur est survenue lors de la recherche des jeux.";
            return View(new List<GameDto>());
        }
    }

    /// <summary>
    /// Action GET : Affiche la fiche détaillée d'un jeu (hybride API RAWG + base locale SQLite).
    /// Calcule les statistiques communautaires (moyenne, répartition des étoiles) et récupère l'état du joueur connecté.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        // 1. Récupération des informations générales du jeu depuis l'API RAWG
        var game = await _gameApiService.GetGameDetailsAsync(id);

        if (game is null)
        {
            return NotFound();
        }

        // 2. Récupération des avis communautaires enregistrés dans notre base de données locale
        var reviews = await _dbContext.Reviews
            .Include(r => r.Member)
            .Where(review => review.GameId == id)
            .OrderByDescending(review => review.ReviewDate)
            .ToListAsync();

        ViewBag.Reviews = reviews;
        // Calcul de la note moyenne des joueurs
        ViewBag.CommunityRating = reviews.Count > 0
            ? reviews.Average(r => r.Rating)
            : (double?)null;
        ViewBag.CommunityReviewCount = reviews.Count;

        // 3. Calcul de la distribution des notes (nombre d'avis pour chaque étoile de 1 à 5)
        var ratingCounts = new Dictionary<int, int>();
        for (int i = 1; i <= 5; i++) ratingCounts[i] = 0;
        foreach (var r in reviews)
        {
            if (r.Rating >= 1 && r.Rating <= 5)
                ratingCounts[(int)Math.Round(r.Rating)]++;
        }
        ViewBag.RatingDistribution = ratingCounts;

        // 4. Si un utilisateur est connecté, récupérer son avis personnel et son statut actuel sur ce jeu
        GameStatus? currentStatus = null;
        Review? userReview = null;
        bool isFavorite = false;
        if (User.Identity?.IsAuthenticated == true)
        {
            var memberIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(memberIdStr, out var memberId))
            {
                userReview = reviews.FirstOrDefault(r => r.MemberId == memberId);

                var statusEntry = await _dbContext.MemberGameStatuses
                    .FirstOrDefaultAsync(s => s.GameId == id && s.MemberId == memberId);
                if (statusEntry != null)
                {
                    currentStatus = statusEntry.Status;
                }

                isFavorite = await _dbContext.Members
                    .Where(m => m.Id == memberId)
                    .SelectMany(m => m.FavoriteGames)
                    .AnyAsync(g => g.Id == id);

                ViewBag.UserLists = await _dbContext.CustomLists
                    .Include(l => l.GamesInList)
                    .Where(l => l.MemberId == memberId)
                    .ToListAsync();
            }
        }
        ViewBag.CurrentStatus = currentStatus;
        ViewBag.UserReview = userReview;
        ViewBag.IsFavorite = isFavorite;

        return View(game);
    }

    /// <summary>
    /// Action POST : Ajoute ou met à jour l'avis (note et commentaire) de l'utilisateur connecté sur un jeu.
    /// Garantit l'existence locale du jeu et recalcule automatiquement la note moyenne globale.
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> AddReview(int id, int rating, string comment)
    {
        var memberIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(memberIdStr, out var memberId))
        {
            return Unauthorized();
        }

        // Étape A : Vérifier si le jeu existe déjà dans la base locale ; sinon, le synchroniser depuis l'API RAWG
        var game = await _dbContext.Games.FirstOrDefaultAsync(g => g.Id == id);
        if (game == null)
        {
            var gameDto = await _gameApiService.GetGameDetailsAsync(id);
            if (gameDto == null) return NotFound();

            game = new Game
            {
                Id = gameDto.Id,
                Title = gameDto.Name,
                Studio = gameDto.Developers.FirstOrDefault()?.Name ?? "Développeur inconnu",
                ReleaseYear = DateTime.TryParse(gameDto.Released, out var date) ? date.Year : 0,
                CoverUrl = gameDto.BackgroundImage ?? "",
                Description = gameDto.Description,
                AverageRating = 0
            };
            _dbContext.Games.Add(game);
        }

        // Étape B : Logique d'Upsert (un seul avis par joueur par jeu : mise à jour si existant, création sinon)
        var existingReview = await _dbContext.Reviews.FirstOrDefaultAsync(r => r.GameId == id && r.MemberId == memberId);
        if (existingReview != null)
        {
            existingReview.Rating = rating;
            existingReview.Comment = comment;
            existingReview.ReviewDate = DateTime.UtcNow;
        }
        else
        {
            var review = new Review
            {
                GameId = id,
                MemberId = memberId,
                Rating = rating,
                Comment = comment,
                ReviewDate = DateTime.UtcNow
            };
            _dbContext.Reviews.Add(review);
        }
        
        await _dbContext.SaveChangesAsync();

        // Étape C : Recalculer dynamiquement la moyenne globale des notes du jeu
        var allRatings = await _dbContext.Reviews.Where(r => r.GameId == id).Select(r => r.Rating).ToListAsync();
        game.AverageRating = allRatings.Any() ? allRatings.Average() : 0;
        
        await _dbContext.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = id });
    }

    /// <summary>
    /// Action POST : Supprime l'avis rédigé par l'utilisateur connecté sur un jeu,
    /// puis met à jour la moyenne des notes.
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> DeleteReview(int id)
    {
        var memberIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(memberIdStr, out var memberId))
        {
            return Unauthorized();
        }

        var review = await _dbContext.Reviews.FirstOrDefaultAsync(r => r.GameId == id && r.MemberId == memberId);
        if (review != null)
        {
            _dbContext.Reviews.Remove(review);
            await _dbContext.SaveChangesAsync();

            // Recalcul de la moyenne après suppression
            var game = await _dbContext.Games.FirstOrDefaultAsync(g => g.Id == id);
            if (game != null)
            {
                var allRatings = await _dbContext.Reviews.Where(r => r.GameId == id).Select(r => r.Rating).ToListAsync();
                game.AverageRating = allRatings.Any() ? allRatings.Average() : 0;
                await _dbContext.SaveChangesAsync();
            }
        }

        return RedirectToAction(nameof(Details), new { id = id });
    }

    /// <summary>
    /// Action POST : Met à jour le statut personnel d'un jeu pour un utilisateur (Wishlist, In Progress, etc.).
    /// Supporte les requêtes asynchrones JavaScript (AJAX) pour éviter le rafraîchissement complet de la page.
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> UpdateGameStatus(int id, GameStatus status)
    {
        var memberIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(memberIdStr, out var memberId))
        {
            return Unauthorized();
        }

        // S'assurer que le jeu est présent dans la table locale Games
        var game = await _dbContext.Games.FirstOrDefaultAsync(g => g.Id == id);
        if (game == null)
        {
            var gameDto = await _gameApiService.GetGameDetailsAsync(id);
            if (gameDto == null) return NotFound();

            game = new Game
            {
                Id = gameDto.Id,
                Title = gameDto.Name,
                Studio = gameDto.Developers.FirstOrDefault()?.Name ?? "Développeur inconnu",
                ReleaseYear = DateTime.TryParse(gameDto.Released, out var date) ? date.Year : 0,
                CoverUrl = gameDto.BackgroundImage ?? "",
                Description = gameDto.Description,
                AverageRating = 0
            };
            _dbContext.Games.Add(game);
        }

        // Mise à jour ou insertion du statut dans la table MemberGameStatuses
        var existingStatus = await _dbContext.MemberGameStatuses
            .FirstOrDefaultAsync(s => s.GameId == id && s.MemberId == memberId);

        if (existingStatus != null)
        {
            existingStatus.Status = status;
        }
        else
        {
            _dbContext.MemberGameStatuses.Add(new MemberGameStatus
            {
                GameId = id,
                MemberId = memberId,
                Status = status
            });
        }

        await _dbContext.SaveChangesAsync();

        // Si la requête provient d'un script JavaScript (AJAX fetch), on renvoie une réponse JSON
        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            return Json(new { success = true, status = status.ToString() });
        }

        // Redirection classique si JavaScript est désactivé
        return RedirectToAction(nameof(Details), new { id = id });
    }
    /// <summary>
    /// Action POST : Toggle le statut favori d'un jeu pour l'utilisateur connecté.
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> ToggleFavorite(int id)
    {
        var memberIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(memberIdStr, out var memberId))
        {
            return Unauthorized();
        }

        // S'assurer que le jeu est présent dans la table locale Games
        var game = await _dbContext.Games.FirstOrDefaultAsync(g => g.Id == id);
        if (game == null)
        {
            var gameDto = await _gameApiService.GetGameDetailsAsync(id);
            if (gameDto == null) return NotFound();

            game = new Game
            {
                Id = gameDto.Id,
                Title = gameDto.Name,
                Studio = gameDto.Developers.FirstOrDefault()?.Name ?? "Développeur inconnu",
                ReleaseYear = DateTime.TryParse(gameDto.Released, out var date) ? date.Year : 0,
                CoverUrl = gameDto.BackgroundImage ?? "",
                Description = gameDto.Description,
                AverageRating = 0
            };
            _dbContext.Games.Add(game);
        }

        var member = await _dbContext.Members
            .Include(m => m.FavoriteGames)
            .FirstOrDefaultAsync(m => m.Id == memberId);

        if (member == null)
        {
            return Unauthorized();
        }

        bool isFavorite = false;
        var existingFavorite = member.FavoriteGames.FirstOrDefault(g => g.Id == id);
        if (existingFavorite != null)
        {
            member.FavoriteGames.Remove(existingFavorite);
        }
        else
        {
            member.FavoriteGames.Add(game);
            isFavorite = true;
        }

        await _dbContext.SaveChangesAsync();

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            return Json(new { success = true, isFavorite = isFavorite });
        }

        return RedirectToAction(nameof(Details), new { id = id });
    }
}
