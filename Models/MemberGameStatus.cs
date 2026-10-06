namespace gamebox.Models;

public class MemberGameStatus
{
    public int MemberId { get; set; }
    public Member Member { get; set; } = null!;

    public int GameId { get; set; }
    public Game Game { get; set; } = null!;

    public GameStatus Status { get; set; }
}
