using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using gamebox.Data;

namespace gamebox.Controllers;

public class ReviewsController : Controller
{
    private readonly AppDbContext _dbContext;

    public ReviewsController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var reviews = await _dbContext.Reviews
            .Include(review => review.Member)
            .Include(review => review.Game)
            .OrderByDescending(review => review.ReviewDate)
            .ToListAsync();

        return View(reviews);
    }
}