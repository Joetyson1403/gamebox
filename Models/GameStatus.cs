namespace gamebox.Models;

public enum GameStatus
{
    ToPlay,     // À jouer
    Playing,    // En cours
    Completed,  // Terminé
    Dropped     // Abandonné
}

public static class GameStatusExtensions
{
    public static string GetDisplayName(this GameStatus status) => status switch
    {
        GameStatus.ToPlay => "À jouer",
        GameStatus.Playing => "En cours",
        GameStatus.Completed => "Terminé",
        GameStatus.Dropped => "Abandonné",
        _ => status.ToString()
    };

    public static string GetIcon(this GameStatus status) => status switch
    {
        GameStatus.ToPlay => "⏳",
        GameStatus.Playing => "🎮",
        GameStatus.Completed => "✅",
        GameStatus.Dropped => "❌",
        _ => "📁"
    };

    public static string GetBadgeClass(this GameStatus status) => status switch
    {
        GameStatus.ToPlay => "status-badge-toplay",
        GameStatus.Playing => "status-badge-playing",
        GameStatus.Completed => "status-badge-completed",
        GameStatus.Dropped => "status-badge-dropped",
        _ => "status-badge-none"
    };
}
