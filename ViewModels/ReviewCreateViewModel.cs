namespace gamebox.ViewModels;

public class ReviewCreateViewModel
{
    public int GameId { get; set; }
    public int MemberId { get; set; }
    public double Rating { get; set; }
    public string? Comment { get; set; }
}
