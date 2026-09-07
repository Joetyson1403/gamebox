namespace gamebox.Models;

public class Review
{
    public int Id { get; set; }
    public double Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public DateTime ReviewDate { get; set; } = DateTime.UtcNow;

    public int MemberId { get; set; }
    public Member Member { get; set; } = null!;

    public int GameId { get; set; }
    public Game Game { get; set; } = null!;
}