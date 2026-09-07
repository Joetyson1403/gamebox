namespace gamebox.Models;

public class Administrator : User
{
    public int AccessLevel { get; set; } = 1;

    public void DeleteGame(Game game) { }
    public void DeleteReview(Review review) { }
}