using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using gamebox.Data;
using gamebox.Models;
using gamebox.Services;
using Microsoft.AspNetCore.Authorization;

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
    public async Task<IActionResult> Details(int id)
    {
        var game = await _gameApiService.GetGameDetailsAsync(id);

        if (game is null)
        {
            return NotFound();
        }

        var reviews = await _dbContext.Reviews
            .Include(r => r.Member)
            .Where(review => review.GameId == id)
            .OrderByDescending(review => review.ReviewDate)
            .ToListAsync();

        ViewBag.Reviews = reviews;
        ViewBag.CommunityRating = reviews.Count > 0
            ? reviews.Average(r => r.Rating)
            : (double?)null;
        ViewBag.CommunityReviewCount = reviews.Count;

        GameStatus? currentStatus = null;
        if (User.Identity?.IsAuthenticated == true)
        {
            var memberIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(memberIdStr, out var memberId))
            {
                var statusEntry = await _dbContext.MemberGameStatuses
                    .FirstOrDefaultAsync(s => s.GameId == id && s.MemberId == memberId);
                if (statusEntry != null)
                {
                    currentStatus = statusEntry.Status;
                }
            }
        }
        ViewBag.CurrentStatus = currentStatus;

        return View(game);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> AddReview(int id, int rating, string comment)
    {
        var memberIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(memberIdStr, out var memberId))
        {
            return Unauthorized();
        }

        // Check if game exists locally
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

        var review = new Review
        {
            GameId = id,
            MemberId = memberId,
            Rating = rating,
            Comment = comment,
            ReviewDate = DateTime.UtcNow
        };
        _dbContext.Reviews.Add(review);
        await _dbContext.SaveChangesAsync(); // Save to generate review Id and ensure it's in DB

        // Recalculate average rating
        var allRatings = await _dbContext.Reviews.Where(r => r.GameId == id).Select(r => r.Rating).ToListAsync();
        game.AverageRating = allRatings.Any() ? allRatings.Average() : 0;
        
        await _dbContext.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = id });
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> UpdateGameStatus(int id, GameStatus status)
    {
        var memberIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(memberIdStr, out var memberId))
        {
            return Unauthorized();
        }

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

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            return Json(new { success = true, status = status.ToString() });
        }

        return RedirectToAction(nameof(Details), new { id = id });
    }
}
