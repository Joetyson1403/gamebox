using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using gamebox.Models;
using gamebox.Services;
using gamebox.ViewModels;

namespace gamebox.Controllers;

/// <summary>
/// Contrôleur gérant les pages statiques d'accueil, de politique de confidentialité et la page d'erreur.
/// </summary>
public class HomeController : Controller
{
    private readonly IGameApiService _gameApiService;

    public HomeController(IGameApiService gameApiService)
    {
        _gameApiService = gameApiService;
    }

    /// <summary>
    /// Action GET : Affiche la page d'accueil du site.
    /// </summary>
    public async Task<IActionResult> Index()
    {
        var searches = await Task.WhenAll(
            _gameApiService.SearchGamesAsync("Starfield"),
            _gameApiService.SearchGamesAsync("Diablo IV"),
            _gameApiService.SearchGamesAsync("Hogwarts Legacy"),
            _gameApiService.SearchGamesAsync("Elden Ring"),
            _gameApiService.SearchGamesAsync(string.Empty));

        GameDto? FindGame(int searchIndex, string title)
        {
            var results = searches[searchIndex];
            return results?.FirstOrDefault(game => string.Equals(game.Name, title, StringComparison.OrdinalIgnoreCase))
                ?? results?.FirstOrDefault();
        }

        var diablo = FindGame(1, "Diablo IV");
        var hogwarts = FindGame(2, "Hogwarts Legacy");

        return View(new HomeViewModel
        {
            TrendingGames = searches[4] ?? new List<GameDto>(),
            EldenRing = FindGame(3, "Elden Ring"),
            Diablo = diablo,
            HogwartsLegacy = hogwarts
        });
    }

    /// <summary>
    /// Action GET : Affiche la page de confidentialité.
    /// </summary>
    public IActionResult Privacy()
    {
        return View();
    }

    /// <summary>
    /// Action GET : Affiche la page d'erreur en cas d'exception non gérée avec l'identifiant de requête.
    /// </summary>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

