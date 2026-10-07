using gamebox.Models;

namespace gamebox.ViewModels;

public class HomeViewModel
{
    public List<GameDto> TrendingGames { get; set; } = new();
    public GameDto? EldenRing { get; set; }
    public GameDto? Diablo { get; set; }
    public GameDto? HogwartsLegacy { get; set; }
}