using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using gamebox.Data;
using gamebox.Models;
using gamebox.Services;
using gamebox.ViewModels;

namespace gamebox.Controllers;

public class GamesController : Controller
{
    private readonly IGameApiService _gameApiService;
    private readonly AppDbContext _db;

    public GamesController(IGameApiService gameApiService, AppDbContext db)
    {
        _gameApiService = gameApiService;
        _db = db;
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
            ViewBag.ErrorMessage =
                "La requête vers l'API a mis trop de temps à répondre (Timeout). Veuillez réessayer.";

            return View(new List<GameDto>());
        }
        catch (Exception)
        {
            ViewBag.ErrorMessage =
                "Une erreur est survenue lors de la recherche des jeux.";

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

        var communityRatings = await _db.Reviews
            .Where(review => review.GameId == id)
            .Select(review => review.Rating)
            .ToListAsync();

        ViewBag.CommunityRating = communityRatings.Count > 0
            ? communityRatings.Average()
            : (double?)null;

        ViewBag.CommunityReviewCount = communityRatings.Count;

        ViewBag.Members = await _db.Members.ToListAsync();
        ViewBag.ReviewModel = new ReviewCreateViewModel
        {
            GameId = id
        };

        return View(game);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddReview(ReviewCreateViewModel vm)
    {
        if (vm == null)
            return BadRequest();

        if (vm.Rating < 1 || vm.Rating > 5)
        {
            ModelState.AddModelError(
                nameof(vm.Rating),
                "La note doit être entre 1 et 5.");
        }

        var memberExists = await _db.Members
            .AnyAsync(m => m.Id == vm.MemberId);

        if (!memberExists)
        {
            ModelState.AddModelError(
                nameof(vm.MemberId),
                "Membre invalide.");
        }

        var gameExists = await _db.Games
            .AnyAsync(g => g.Id == vm.GameId);

        if (!gameExists)
        {
            ModelState.AddModelError(
                nameof(vm.GameId),
                "Jeu introuvable.");
        }

        if (!ModelState.IsValid)
        {
            var game = await _gameApiService.GetGameDetailsAsync(vm.GameId);

            ViewBag.Members = await _db.Members.ToListAsync();
            ViewBag.ReviewModel = vm;

            return View("Details", game);
        }

        var review = new Review
        {
            Rating = vm.Rating,
            Comment = vm.Comment ?? string.Empty,
            ReviewDate = DateTime.UtcNow,
            MemberId = vm.MemberId,
            GameId = vm.GameId
        };

        _db.Reviews.Add(review);
        await _db.SaveChangesAsync();

        return RedirectToAction(
            nameof(Details),
            new { id = vm.GameId });
    }
}