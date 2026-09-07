namespace gamebox.Models;

public class CustomList
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public int MemberId { get; set; }
    public Member Member { get; set; } = null!;

    public List<Game> GamesInList { get; set; } = new();

    // La méthode pointe bien sur Game maintenant
    public void AddGame(Game game) => GamesInList.Add(game); 
}