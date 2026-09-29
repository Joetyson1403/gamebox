namespace gamebox.Models;

public class GameDetailsViewModel
{
    public Game Game { get; set; } = null!;
    public Member? ActiveMember { get; set; }
    public GameStatus? CurrentStatus { get; set; }
}
