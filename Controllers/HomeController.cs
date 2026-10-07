using System.Diagnostics;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using gamebox.Data;
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
    private readonly AppDbContext _dbContext;

    public HomeController(IGameApiService gameApiService, AppDbContext dbContext)
    {
        _gameApiService = gameApiService;
        _dbContext = dbContext;
    }

    /// <summary>
    /// Action GET : Affiche la page d'accueil du site.
    /// </summary>
    public async Task<IActionResult> Index()
    {
        string? profileAvatarSource = null;
        var memberIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (int.TryParse(memberIdValue, out var memberId))
        {
            var avatar = await _dbContext.Members
                .Where(member => member.Id == memberId)
                .Select(member => new { member.AvatarImage, member.AvatarUrl, member.Username })
                .FirstOrDefaultAsync();

            if (avatar?.AvatarImage is { Length: > 0 })
            {
                profileAvatarSource = $"data:image/jpeg;base64,{Convert.ToBase64String(avatar.AvatarImage)}";
            }
            else if (!string.IsNullOrWhiteSpace(avatar?.AvatarUrl))
            {
                profileAvatarSource = avatar.AvatarUrl;
            }
            else if (!string.IsNullOrWhiteSpace(avatar?.Username))
            {
                profileAvatarSource = $"https://api.dicebear.com/7.x/avataaars/svg?seed={Uri.EscapeDataString(avatar.Username)}";
            }
        }

        var trendingTask = _gameApiService.SearchGamesAsync(string.Empty);
        var hogwartsTask = _gameApiService.SearchGamesAsync("Hogwarts Legacy");
        var recentReviewsTask = _dbContext.Reviews
            .Include(review => review.Member)
            .Include(review => review.Game)
            .OrderByDescending(review => review.ReviewDate)
            .Take(3)
            .ToListAsync();

        await Task.WhenAll(trendingTask, hogwartsTask, recentReviewsTask);

        return View(new HomeViewModel
        {
            ProfileAvatarSource = profileAvatarSource,
            TrendingGames = trendingTask.Result ?? new List<GameDto>(),
            RecentReviews = recentReviewsTask.Result,
            HogwartsLegacy = hogwartsTask.Result?.FirstOrDefault(game => string.Equals(game.Name, "Hogwarts Legacy", StringComparison.OrdinalIgnoreCase))
                ?? hogwartsTask.Result?.FirstOrDefault()
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

