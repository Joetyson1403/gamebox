namespace gamebox.Models;

public class Member : User
{
    public List<Review> MyReviews { get; set; } = new();
    public List<Game> ToPlay { get; set; } = new();
    public List<Game> FavoriteGames { get; set; } = new();
    // La liste des jeux terminés/joués
    public List<Game> PlayedGames { get; set; } = new(); 
    public List<CustomList> MyCustomLists { get; set; } = new();

    public void AddToPlaylist(Game game) => ToPlay.Add(game);
    // Méthode pour ajouter le statut
    public void MarkAsPlayed(Game game) => PlayedGames.Add(game); 
    public void WriteReview(Review review) => MyReviews.Add(review);
    public void CreateList(string title) => MyCustomLists.Add(new CustomList { Title = title, Member = this });
}