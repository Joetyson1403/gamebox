using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using gamebox.Data;
using gamebox.Models;
using gamebox.ViewModels;

namespace gamebox.Controllers;

public class GamesController : Controller
{
    private readonly AppDbContext _db;

    public GamesController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Details(int id)
    {
        var game = await _db.Games
            .Include(g => g.Reviews)
                .ThenInclude(r => r.Member)
            .FirstOrDefaultAsync(g => g.Id == id);

        if (game == null)
            return NotFound();

        var members = await _db.Members.ToListAsync();
        ViewBag.Members = members;
        ViewBag.ReviewModel = new ReviewCreateViewModel { GameId = id };

        return View(game);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddReview(ReviewCreateViewModel vm)
    {
        if (vm == null)
            return BadRequest();

        // Basic server-side validation
        if (vm.Rating < 1 || vm.Rating > 5)
            ModelState.AddModelError(nameof(vm.Rating), "La note doit être entre 1 et 5.");

        var memberExists = await _db.Members.AnyAsync(m => m.Id == vm.MemberId);
        if (!memberExists)
            ModelState.AddModelError(nameof(vm.MemberId), "Membre invalide.");

        var gameEntity = await _db.Games.FirstOrDefaultAsync(g => g.Id == vm.GameId);
        if (gameEntity == null)
            ModelState.AddModelError(nameof(vm.GameId), "Jeu introuvable.");

        if (!ModelState.IsValid)
        {
            var game = await _db.Games
                .Include(g => g.Reviews).ThenInclude(r => r.Member)
                .FirstOrDefaultAsync(g => g.Id == vm.GameId);
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

        // Recalculate average rating from real reviews
        var avg = await _db.Reviews.Where(r => r.GameId == vm.GameId).AverageAsync(r => r.Rating);
        gameEntity.AverageRating = avg;
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = vm.GameId });
    }
}
