using System.Text.Json;
using gamebox.Models;

namespace gamebox.Services;

/// <summary>
/// Interface définissant les opérations d'accès au catalogue externe de jeux (API RAWG).
/// </summary>
public interface IGameApiService
{
    /// <summary>
    /// Recherche des jeux par mot-clé ou retourne les jeux populaires du moment.
    /// </summary>
    /// <param name="query">Terme recherché par l'utilisateur (facultatif).</param>
    /// <returns>Liste de jeux simplifiés (GameDto) ou null en cas d'erreur.</returns>
    Task<List<GameDto>?> SearchGamesAsync(string query);

    /// <summary>
    /// Récupère la fiche complète et détaillée d'un jeu à partir de son identifiant RAWG.
    /// </summary>
    /// <param name="id">Identifiant unique du jeu chez RAWG.</param>
    /// <returns>Détails du jeu (RawgGameDetailsDto) ou null si introuvable.</returns>
    Task<RawgGameDetailsDto?> GetGameDetailsAsync(int id);
}

/// <summary>
/// Service HTTP responsable de la communication avec l'API REST externe RAWG.io.
/// Gère la construction des requêtes, la désérialisation JSON et les erreurs réseau.
/// </summary>
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
        
        // Configuration de l'URL de base de l'API RAWG depuis appsettings.json
        var baseUrl = _configuration["RawgApi:BaseUrl"];
        if (!string.IsNullOrEmpty(baseUrl))
        {
            _httpClient.BaseAddress = new Uri(baseUrl);
        }
    }

    /// <summary>
    /// Recherche des jeux sur RAWG avec filtres sur les plateformes principales (PC, PlayStation, Xbox, Nintendo).
    /// </summary>
    public async Task<List<GameDto>?> SearchGamesAsync(string query)
    {
        try
        {
            var apiKey = _configuration["RawgApi:ApiKey"];
            string requestUri;
            
            if (string.IsNullOrWhiteSpace(query))
            {
                // Si aucune recherche n'est saisie : afficher les jeux les plus ajoutés/populaires du moment
                requestUri = $"games?key={apiKey}&exclude_additions=true&ordering=-added&parent_platforms=1,2,3,7";
            }
            else
            {
                // Recherche précise basée sur le texte saisi par l'utilisateur
                requestUri = $"games?search={Uri.EscapeDataString(query)}&key={apiKey}&exclude_additions=true&search_precise=true&parent_platforms=1,2,3,7";
            }
            
            // Envoi de la requête HTTP asynchrone vers l'API
            var response = await _httpClient.GetAsync(requestUri);
            
            if (response.IsSuccessStatusCode)
            {
                // Lecture et désérialisation du JSON retourné par RAWG
                var content = await response.Content.ReadAsStringAsync();
                var searchResponse = JsonSerializer.Deserialize<RawgSearchResponse>(content);
                
                // Filtrer les jeux pour ne garder que ceux ayant une note > 0 (élimine les clones et fan-games vides)
                var filteredResults = searchResponse?.Results?
                    .Where(g => g.Rating > 0)
                    .ToList();
                    
                return filteredResults;
            }
            else
            {
                _logger.LogWarning($"Échec de la requête RAWG avec le code : {response.StatusCode}");
                return null;
            }
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger.LogError(ex, "Délai d'attente dépassé (Timeout) avec l'API RAWG.");
            throw new Exception("Timeout");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Une erreur inattendue est survenue lors de la récupération des données de l'API.");
            return null;
        }
    }

    /// <summary>
    /// Récupère les données complètes (description, studios, date de sortie, jaquettes) d'un jeu précis.
    /// </summary>
    public async Task<RawgGameDetailsDto?> GetGameDetailsAsync(int id)
    {
        try
        {
            var apiKey = _configuration["RawgApi:ApiKey"];
            // Appel direct à l'endpoint spécifique /games/{id}
            var response = await _httpClient.GetAsync($"games/{id}?key={apiKey}");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("RAWG a répondu avec le code {StatusCode} pour le jeu {GameId}.", response.StatusCode, id);
                return null;
            }

            // Désérialisation du corps de réponse JSON en objet modèle DTO
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<RawgGameDetailsDto>(content);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur pendant la récupération des détails du jeu {GameId}.", id);
            return null;
        }
    }
}
