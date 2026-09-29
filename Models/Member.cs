namespace gamebox.Models;

public class Member : User
{
    public List<Review> MyReviews { get; set; } = new();
    public List<Game> ToPlay { get; set; } = new();
    public List<Game> FavoriteGames { get; set; } = new();
    // La liste des jeux terminés/joués
    public List<Game> PlayedGames { get; set; } = new(); 
    public List<CustomList> MyCustomLists { get; set; } = new();
    public List<UserGameStatus> GameStatuses { get; set; } = new();

    public void AddToPlaylist(Game game) => ToPlay.Add(game);
    // Méthode pour ajouter le statut
    public void MarkAsPlayed(Game game) => PlayedGames.Add(game); 
    public void WriteReview(Review review) => MyReviews.Add(review);
    public void CreateList(string title) => MyCustomLists.Add(new CustomList { Title = title, Member = this });

    public void SetGameStatus(Game game, GameStatus status)
    {
        var entry = GameStatuses.FirstOrDefault(gs => gs.GameId == game.Id);
        if (entry == null)
        {
            GameStatuses.Add(new UserGameStatus { Member = this, Game = game, Status = status });
        }
        else
        {
            entry.Status = status;
            entry.UpdatedAt = DateTime.UtcNow;
        }

        // Synchronisation avec les listes UML héritées (ToPlay / PlayedGames)
        if (status == GameStatus.ToPlay)
        {
            if (!ToPlay.Any(g => g.Id == game.Id)) ToPlay.Add(game);
            PlayedGames.RemoveAll(g => g.Id == game.Id);
        }
        else if (status == GameStatus.Completed)
        {
            if (!PlayedGames.Any(g => g.Id == game.Id)) PlayedGames.Add(game);
            ToPlay.RemoveAll(g => g.Id == game.Id);
        }
        else
        {
            ToPlay.RemoveAll(g => g.Id == game.Id);
            PlayedGames.RemoveAll(g => g.Id == game.Id);
        }
    }
}