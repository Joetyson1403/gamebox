using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using gamebox.Data;
using gamebox.Models;

namespace gamebox.Controllers;

[Authorize]
public class ProfileController : Controller
{
    private readonly AppDbContext _dbContext;

    public ProfileController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IActionResult> Index()
    {
        var memberIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(memberIdStr, out var memberId))
        {
            return Unauthorized();
        }

        var member = await _dbContext.Members
            .Include(m => m.FavoriteGames)
            .Include(m => m.MyCustomLists)
            .Include(m => m.GameStatuses)
                .ThenInclude(gs => gs.Game)
            .Include(m => m.MyReviews)
                .ThenInclude(r => r.Game)
            .FirstOrDefaultAsync(m => m.Id == memberId);

        if (member == null)
        {
            return NotFound();
        }

        // We will pass the member to the view, which now has MyReviews included.
        return View(member);
    }

    [HttpPost]
    public async Task<IActionResult> CreateList(string title, string description)
    {
        var memberIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(memberIdStr, out var memberId))
        {
            return Unauthorized();
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            TempData["ErrorMessage"] = "Le titre de la liste est obligatoire.";
            return RedirectToAction(nameof(Index));
        }

        var newList = new CustomList
        {
            Title = title,
            Description = description ?? string.Empty,
            MemberId = memberId
        };

        _dbContext.CustomLists.Add(newList);
        await _dbContext.SaveChangesAsync();

        TempData["SuccessMessage"] = "Liste créée avec succès.";
        if (Request.Headers["Referer"].ToString().Contains("/Profile/Lists"))
        {
            return RedirectToAction(nameof(Lists));
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit()
    {
        var memberIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(memberIdStr, out var memberId))
        {
            return Unauthorized();
        }

        var member = await _dbContext.Members
            .Include(m => m.FavoriteGames)
            .Include(m => m.MyCustomLists)
            .Include(m => m.GameStatuses)
                .ThenInclude(gs => gs.Game)
            .FirstOrDefaultAsync(m => m.Id == memberId);

        if (member == null)
        {
            return NotFound();
        }

        return View(member);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(IFormFile avatarFile)
    {
        var memberIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(memberIdStr, out var memberId))
        {
            return Unauthorized();
        }

        var member = await _dbContext.Members.FirstOrDefaultAsync(m => m.Id == memberId);
        if (member != null)
        {
            if (avatarFile != null && avatarFile.Length > 0)
            {
                using (var ms = new MemoryStream())
                {
                    await avatarFile.CopyToAsync(ms);
                    member.AvatarImage = ms.ToArray();
                }
            }

            await _dbContext.SaveChangesAsync();
            TempData["SuccessMessage"] = "Profil mis à jour avec succès.";
        }

        return RedirectToAction(nameof(Index));
    }
    public async Task<IActionResult> Favorites()
    {
        var memberIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(memberIdStr, out var memberId))
        {
            return Unauthorized();
        }

        var member = await _dbContext.Members
            .Include(m => m.FavoriteGames)
            .FirstOrDefaultAsync(m => m.Id == memberId);

        if (member == null)
        {
            return NotFound();
        }

        return View(member);
    }

    public async Task<IActionResult> Activity()
    {
        var memberIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(memberIdStr, out var memberId))
        {
            return Unauthorized();
        }

        var member = await _dbContext.Members
            .Include(m => m.GameStatuses)
                .ThenInclude(gs => gs.Game)
            .Include(m => m.MyReviews)
                .ThenInclude(r => r.Game)
            .FirstOrDefaultAsync(m => m.Id == memberId);

        if (member == null)
        {
            return NotFound();
        }

        return View(member);
    }
    public async Task<IActionResult> Collection()
    {
        var memberIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(memberIdStr, out var memberId))
        {
            return Unauthorized();
        }

        var member = await _dbContext.Members
            .Include(m => m.GameStatuses)
                .ThenInclude(gs => gs.Game)
            .Include(m => m.MyReviews)
            .FirstOrDefaultAsync(m => m.Id == memberId);

        if (member == null)
        {
            return NotFound();
        }

        return View(member);
    }
    public async Task<IActionResult> Lists()
    {
        var memberIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(memberIdStr, out var memberId))
        {
            return Unauthorized();
        }

        var member = await _dbContext.Members
            .Include(m => m.MyCustomLists)
            .FirstOrDefaultAsync(m => m.Id == memberId);

        if (member == null)
        {
            return NotFound();
        }

        return View(member);
    }

    [HttpPost]
    public async Task<IActionResult> DeleteList(int id)
    {
        var memberIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(memberIdStr, out var memberId))
        {
            return Unauthorized();
        }

        var list = await _dbContext.CustomLists.FirstOrDefaultAsync(l => l.Id == id && l.MemberId == memberId);
        if (list != null)
        {
            _dbContext.CustomLists.Remove(list);
            await _dbContext.SaveChangesAsync();
            return Json(new { success = true });
        }
        
        return Json(new { success = false });
    }

    public async Task<IActionResult> ListDetails(int id)
    {
        var memberIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(memberIdStr, out var memberId))
        {
            return Unauthorized();
        }

        var list = await _dbContext.CustomLists
            .Include(l => l.GamesInList)
            .FirstOrDefaultAsync(l => l.Id == id && l.MemberId == memberId);

        if (list == null) return NotFound();

        return View(list);
    }

    [HttpPost]
    public async Task<IActionResult> RemoveGameFromList(int listId, int gameId)
    {
        var memberIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(memberIdStr, out var memberId))
        {
            return Unauthorized();
        }

        var list = await _dbContext.CustomLists
            .Include(l => l.GamesInList)
            .FirstOrDefaultAsync(l => l.Id == listId && l.MemberId == memberId);

        if (list == null) return NotFound();

        var game = list.GamesInList.FirstOrDefault(g => g.Id == gameId);
        if (game != null)
        {
            list.GamesInList.Remove(game);
            await _dbContext.SaveChangesAsync();
        }

        return RedirectToAction(nameof(ListDetails), new { id = listId });
    }

    [HttpPost]
    public async Task<IActionResult> AddGameToList(int listId, int gameId)
    {
        var memberIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(memberIdStr, out var memberId))
        {
            return Unauthorized();
        }

        var list = await _dbContext.CustomLists
            .Include(l => l.GamesInList)
            .FirstOrDefaultAsync(l => l.Id == listId && l.MemberId == memberId);

        if (list == null) return NotFound();

        var game = await _dbContext.Games.FindAsync(gameId);
        if (game != null && !list.GamesInList.Any(g => g.Id == gameId))
        {
            list.GamesInList.Add(game);
            await _dbContext.SaveChangesAsync();
        }

        return RedirectToAction("Details", "Games", new { id = gameId });
    }
}
