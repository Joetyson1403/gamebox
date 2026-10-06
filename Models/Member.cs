namespace gamebox.Models;

public class Member : User
{
    public List<Review> MyReviews { get; set; } = new();
    public List<Game> FavoriteGames { get; set; } = new();
    public List<MemberGameStatus> GameStatuses { get; set; } = new();
    public List<CustomList> MyCustomLists { get; set; } = new();

    public void WriteReview(Review review) => MyReviews.Add(review);
    public void CreateList(string title) => MyCustomLists.Add(new CustomList { Title = title, Member = this });
}