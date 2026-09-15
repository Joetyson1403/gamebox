using System.Text.Json;
using gamebox.Models;

namespace gamebox.Services;

public interface IGameApiService
{
    Task<List<GameDto>?> SearchGamesAsync(string query);
}

public class GameApiService : IGameApiService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<GameApiService> _logger;

    public GameApiService(HttpClient httpClient, IConfiguration configuration, ILogger<GameApiService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
        
        var baseUrl = _configuration["RawgApi:BaseUrl"];
        if (!string.IsNullOrEmpty(baseUrl))
        {
            _httpClient.BaseAddress = new Uri(baseUrl);
        }
    }

    public async Task<List<GameDto>?> SearchGamesAsync(string query)
    {
        try
        {
            var apiKey = _configuration["RawgApi:ApiKey"];
            string requestUri;
            
            if (string.IsNullOrWhiteSpace(query))
            {
                // Trending now
                requestUri = $"games?key={apiKey}&exclude_additions=true&ordering=-added&parent_platforms=1,2,3,7";
            }
            else
            {
                // Search query
                requestUri = $"games?search={Uri.EscapeDataString(query)}&key={apiKey}&exclude_additions=true&search_precise=true&parent_platforms=1,2,3,7";
            }
            
            var response = await _httpClient.GetAsync(requestUri);
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var searchResponse = JsonSerializer.Deserialize<RawgSearchResponse>(content);
                
                // Filtrer les jeux pour ne garder que ceux qui ont une vraie note (exclut les fan games à 0)
                var filteredResults = searchResponse?.Results?
                    .Where(g => g.Rating > 0)
                    .ToList();
                    
                return filteredResults;
            }
            else
            {
                _logger.LogWarning($"API request failed with status code: {response.StatusCode}");
                return null;
            }
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger.LogError(ex, "API request timed out.");
            throw new Exception("Timeout");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while fetching data from API.");
            return null;
        }
    }
}
