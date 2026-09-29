using System.Text.Json.Serialization;

namespace gamebox.Models;

public class RawgSearchResponse
{
    [JsonPropertyName("count")]
    public int Count { get; set; }

    [JsonPropertyName("next")]
    public string? Next { get; set; }

    [JsonPropertyName("previous")]
    public string? Previous { get; set; }

    [JsonPropertyName("results")]
    public List<GameDto> Results { get; set; } = new List<GameDto>();
}

public class GameDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("background_image")]
    public string? BackgroundImage { get; set; }

    [JsonPropertyName("released")]
    public string? Released { get; set; }

    [JsonPropertyName("rating")]
    public double Rating { get; set; }
}

public class RawgGameDetailsDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("background_image")]
    public string? BackgroundImage { get; set; }

    [JsonPropertyName("description_raw")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("released")]
    public string? Released { get; set; }

    [JsonPropertyName("developers")]
    public List<RawgDeveloperDto> Developers { get; set; } = new();

    [JsonPropertyName("genres")]
    public List<RawgGenreDto> Genres { get; set; } = new();
}

public class RawgDeveloperDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

public class RawgGenreDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}
