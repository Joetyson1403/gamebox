using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using gamebox.Data;
using gamebox.Models;
using gamebox.Services;

namespace gamebox.Controllers;

public class GamesController : Controller
{
    private readonly IGameApiService _gameApiService;
    private readonly AppDbContext _dbContext;

    public GamesController(IGameApiService gameApiService, AppDbContext dbContext)
    {
        _gameApiService = gameApiService;
        _dbContext = dbContext;
    }

    // GET: /Games/ or /Games/Index
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var games = await _dbContext.Games.ToListAsync();
        return View(games);
    }

    [HttpGet]
    public async Task<IActionResult> Search(string? query)
    {
        ViewBag.Query = query;

        try
        {
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

    [HttpGet]
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            var firstGame = await _dbContext.Games.FirstOrDefaultAsync();
            if (firstGame == null)
            {
                return NotFound("Aucun jeu trouvé.");
            }
            id = firstGame.Id;
        }

        RawgGameDetailsDto? game = null;

        try
        {
            game = await _gameApiService.GetGameDetailsAsync(id.Value);
        }
        catch
        {
            // En cas d'erreur API, tentative de chargement local
        }

        if (game == null)
        {
            var localGame = await _dbContext.Games.FirstOrDefaultAsync(g => g.Id == id.Value);
            if (localGame == null)
            {
                return NotFound();
            }

            game = new RawgGameDetailsDto
            {
                Id = localGame.Id,
                Name = localGame.Title,
                BackgroundImage = localGame.CoverUrl,
                Description = localGame.Description,
                Released = localGame.ReleaseYear > 0 ? localGame.ReleaseYear.ToString() : null,
                Developers = !string.IsNullOrEmpty(localGame.Studio)
                    ? new List<RawgDeveloperDto> { new() { Name = localGame.Studio } }
                    : new List<RawgDeveloperDto>()
            };
        }

        var communityRatings = await _dbContext.Reviews
            .Where(review => review.GameId == id.Value)
            .Select(review => review.Rating)
            .ToListAsync();

        ViewBag.CommunityRating = communityRatings.Count > 0
            ? communityRatings.Average()
            : (double?)null;
        ViewBag.CommunityReviewCount = communityRatings.Count;

        // Récupérer l'utilisateur actif et son statut pour ce jeu (GBX-8)
        var activeMember = await GetActiveMemberAsync();
        GameStatus? currentStatus = null;

        if (activeMember != null)
        {
            var statusEntry = await _dbContext.UserGameStatuses
                .FirstOrDefaultAsync(s => s.MemberId == activeMember.Id && s.GameId == id.Value);

            currentStatus = statusEntry?.Status;
        }

        ViewBag.ActiveMember = activeMember;
        ViewBag.CurrentStatus = currentStatus;

        return View(game);
    }

    // POST: /Games/SetStatus (GBX-31 & GBX-32)
    [HttpPost]
    public async Task<IActionResult> SetStatus(int gameId, GameStatus? status, string? gameTitle = null, string? coverUrl = null)
    {
        var game = await _dbContext.Games.FindAsync(gameId);
        if (game == null)
        {
            // S'assurer que le jeu existe dans SQLite pour satisfaire la clé étrangère
            game = new Game
            {
                Id = gameId,
                Title = !string.IsNullOrWhiteSpace(gameTitle) ? gameTitle : $"Jeu #{gameId}",
                CoverUrl = coverUrl ?? string.Empty
            };
            _dbContext.Games.Add(game);
            await _dbContext.SaveChangesAsync();
        }

        var activeMember = await GetActiveMemberAsync();
        if (activeMember == null)
        {
            return Unauthorized(new { success = false, message = "Aucun utilisateur actif" });
        }

        var statusEntry = await _dbContext.UserGameStatuses
            .FirstOrDefaultAsync(s => s.MemberId == activeMember.Id && s.GameId == gameId);

        if (status.HasValue)
        {
            if (statusEntry == null)
            {
                statusEntry = new UserGameStatus
                {
                    MemberId = activeMember.Id,
                    GameId = gameId,
                    Status = status.Value,
                    UpdatedAt = DateTime.UtcNow
                };
                _dbContext.UserGameStatuses.Add(statusEntry);
            }
            else
            {
                statusEntry.Status = status.Value;
                statusEntry.UpdatedAt = DateTime.UtcNow;
            }

            // Mettre à jour les listes UML du membre (GBX-8)
            activeMember.SetGameStatus(game, status.Value);
        }
        else
        {
            // Supprimer le statut si null
            if (statusEntry != null)
            {
                _dbContext.UserGameStatuses.Remove(statusEntry);
            }
            activeMember.ToPlay.RemoveAll(g => g.Id == gameId);
            activeMember.PlayedGames.RemoveAll(g => g.Id == gameId);
        }

        await _dbContext.SaveChangesAsync();

        // Réponse AJAX (GBX-32)
        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
            Request.Headers.Accept.ToString().Contains("application/json"))
        {
            var hasStatus = status.HasValue;
            var displayName = hasStatus ? status!.Value.GetDisplayName() : "Définir un statut";
            var icon = hasStatus ? status!.Value.GetIcon() : "+";
            var badgeClass = hasStatus ? status!.Value.GetBadgeClass() : "status-badge-none";

            return Json(new
            {
                success = true,
                gameId,
                hasStatus,
                status = hasStatus ? status!.Value.ToString() : null,
                displayName,
                icon,
                badgeClass,
                buttonText = $"{icon} {displayName}"
            });
        }

        return RedirectToAction(nameof(Details), new { id = gameId });
    }

    private async Task<Member?> GetActiveMemberAsync()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdClaim, out var userId))
        {
            var member = await _dbContext.Members
                .Include(m => m.ToPlay)
                .Include(m => m.PlayedGames)
                .Include(m => m.GameStatuses)
                .FirstOrDefaultAsync(m => m.Id == userId);

            if (member != null) return member;
        }

        // Utilisateur par défaut (Alex)
        return await _dbContext.Members
            .Include(m => m.ToPlay)
            .Include(m => m.PlayedGames)
            .Include(m => m.GameStatuses)
            .FirstOrDefaultAsync();
    }
}
