using Microsoft.AspNetCore.Mvc;
using gamebox.Services;
using gamebox.Models;

namespace gamebox.Controllers;

public class GamesController : Controller
{
    private readonly IGameApiService _gameApiService;

    public GamesController(IGameApiService gameApiService)
    {
        _gameApiService = gameApiService;
    }

    [HttpGet]
    public async Task<IActionResult> Search(string? query)
    {
        ViewBag.Query = query;

        try
        {
            var games = await _gameApiService.SearchGamesAsync(query);
            
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
}
