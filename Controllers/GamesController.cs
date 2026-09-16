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

        var communityRatings = await _dbContext.Reviews
            .Where(review => review.GameId == id)
            .Select(review => review.Rating)
            .ToListAsync();

        ViewBag.CommunityRating = communityRatings.Count > 0
            ? communityRatings.Average()
            : (double?)null;
        ViewBag.CommunityReviewCount = communityRatings.Count;

        return View(game);
    }
}
