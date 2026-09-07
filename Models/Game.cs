namespace gamebox.Models;

public class Game
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Studio { get; set; } = string.Empty;
    public int ReleaseYear { get; set; }
    public string CoverUrl { get; set; } = string.Empty;
    public double AverageRating { get; set; }
    public string Description { get; set; } = string.Empty;

    public List<Review> Reviews { get; set; } = new();
    public List<Member> FavoritedBy { get; set; } = new();
    public List<Member> InToPlayOf { get; set; } = new();
    // Lien inverse
    public List<Member> PlayedBy { get; set; } = new(); 
    public List<CustomList> CustomLists { get; set; } = new();
}