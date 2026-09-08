using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using gamebox.Data;
using gamebox.Models;

namespace gamebox.Controllers;

public class GamesController : Controller
{
    private readonly AppDbContext _context;

    public GamesController(AppDbContext context)
    {
        _context = context;
    }

    // GET: /Games/ or /Games/Index
    public async Task<IActionResult> Index()
    {
        var games = await _context.Games.ToListAsync();
        return View(games);
    }

    // GET: /Games/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            var firstGame = await _context.Games.FirstOrDefaultAsync();
            if (firstGame == null) return NotFound("Aucun jeu trouvé.");
            id = firstGame.Id;
        }

        var game = await _context.Games
            .Include(g => g.Reviews)
            .ThenInclude(r => r.Member)
            .FirstOrDefaultAsync(g => g.Id == id.Value);

        if (game == null)
        {
            return NotFound();
        }

        // Récupérer l'utilisateur actif (Alex par défaut)
        var activeMember = await GetActiveMemberAsync();

        // Récupérer le statut actuel du jeu pour cet utilisateur
        GameStatus? currentStatus = null;
        if (activeMember != null)
        {
            var statusEntry = await _context.UserGameStatuses
                .FirstOrDefaultAsync(s => s.MemberId == activeMember.Id && s.GameId == game.Id);

            if (statusEntry != null)
            {
                currentStatus = statusEntry.Status;
            }
        }

        var viewModel = new GameDetailsViewModel
        {
            Game = game,
            ActiveMember = activeMember,
            CurrentStatus = currentStatus
        };

        return View(viewModel);
    }

    // POST: /Games/SetStatus (GBX-31 & GBX-32)
    [HttpPost]
    public async Task<IActionResult> SetStatus(int gameId, GameStatus? status)
    {
        var game = await _context.Games.FindAsync(gameId);
        if (game == null)
        {
            return NotFound(new { success = false, message = "Jeu introuvable" });
        }

        var activeMember = await GetActiveMemberAsync();
        if (activeMember == null)
        {
            return Unauthorized(new { success = false, message = "Aucun utilisateur actif" });
        }

        var statusEntry = await _context.UserGameStatuses
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
                _context.UserGameStatuses.Add(statusEntry);
            }
            else
            {
                statusEntry.Status = status.Value;
                statusEntry.UpdatedAt = DateTime.UtcNow;
            }

            // Mettre à jour les listes UML du membre
            activeMember.SetGameStatus(game, status.Value);
        }
        else
        {
            // Supprimer le statut si null
            if (statusEntry != null)
            {
                _context.UserGameStatuses.Remove(statusEntry);
            }
            activeMember.ToPlay.RemoveAll(g => g.Id == gameId);
            activeMember.PlayedGames.RemoveAll(g => g.Id == gameId);
        }

        await _context.SaveChangesAsync();

        // Si la requête est en AJAX, renvoyer du JSON avec les informations visuelles
        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" || Request.Headers.Accept.ToString().Contains("application/json"))
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
        return await _context.Members
            .Include(m => m.ToPlay)
            .Include(m => m.PlayedGames)
            .Include(m => m.GameStatuses)
            .FirstOrDefaultAsync();
    }
}
