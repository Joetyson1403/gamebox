using gamebox.Models;

namespace gamebox.ViewModels;

public class HomeViewModel
{
    public string? ProfileAvatarSource { get; set; }
    public List<GameDto> TrendingGames { get; set; } = new();
    public List<Review> RecentReviews { get; set; } = new();
    public GameDto? HogwartsLegacy { get; set; }
}