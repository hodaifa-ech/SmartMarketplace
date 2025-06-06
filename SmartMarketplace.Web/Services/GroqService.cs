using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using SmartMarketplace.Web.ViewModels;

namespace SmartMarketplace.Web.Services;



public class GroqService : IGroqService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly string _apiKey;

    public GroqService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _apiKey = _configuration["Groq:ApiKey"] ?? throw new InvalidOperationException("Groq API key not configured.");
    }

    // La signature de la méthode est simplifiée
    public async Task<string> GenerateMissionJsonAsync(string userPrompt)
    {
        var httpClient = _httpClientFactory.CreateClient("Groq");
        
        // --- NOUVEAU SYSTEM PROMPT PLUS INTELLIGENT ---
        var systemPrompt = @"
Tu es un assistant expert multilingue pour la création de fiches de mission.
Ta tâche est de générer une fiche de mission en JSON en suivant ces règles STRICTES:

1.  **Priorité 1 : Détecter une langue demandée.** Analyse la demande de l'utilisateur. S'il mentionne explicitement une langue (ex: 'en chinois', 'in English', 'en español'), tu DOIS générer TOUTE la fiche mission dans cette langue.
2.  **Priorité 2 : Détection automatique.** Si aucune langue n'est explicitement demandée, détecte la langue principale de la demande et génère la fiche mission dans cette langue.
3.  **Format de sortie :** Réponds UNIQUEMENT avec l'objet JSON, sans aucun texte, commentaire, ou explication avant ou après. Les champs `country` et `city` doivent rester en anglais pour la cohérence des données, mais tous les autres champs textuels (`title`, `description`, `domain`, `position`) doivent être dans la langue de sortie déterminée.

Voici le format JSON à respecter :
{
  ""title"": ""Titre concis et accrocheur"",
  ""description"": ""Description détaillée. Utilise \n pour les sauts de ligne."",
  ""country"": ""Nom du pays en anglais"",
  ""city"": ""Nom de la ville en anglais"",
  ""workMode"": ""Un parmi: REMOTE, ONSITE, HYBRID"",
  ""duration"": ""Durée (nombre)"",
  ""durationType"": ""Unité de durée (MONTH, YEAR)"",
  ""startImmediately"": true/false,
  ""startDate"": ""Date yyyy-MM-dd (si startImmediately=false, sinon null)"",
  ""experienceYear"": ""Un parmi: 0-3, 3-7, 7-12, 12+"",
  ""contractType"": ""Un parmi: FORFAIT, REGIE"",
  ""estimatedDailyRate"": ""TJM en euros (nombre uniquement)"",
  ""domain"": ""Domaine d'activité principal"",
  ""position"": ""Intitulé du poste"",
  ""requiredExpertises"": [""expertise1"", ""expertise2""]
}
";
        var requestPayload = new
        {
            messages = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userPrompt }
            },
            model = "llama3-70b-8192",
            temperature = 0.5,
            max_tokens = 2048,
            top_p = 1,
            stop = (string?)null,
            stream = false
        };

        var content = new StringContent(JsonSerializer.Serialize(requestPayload), Encoding.UTF8, "application/json");
        var response = await httpClient.PostAsync("openai/v1/chat/completions", content);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Groq API call failed with status code {response.StatusCode}: {errorBody}");
        }

        var responseBody = await response.Content.ReadAsStringAsync();
        
        try
        {
            var jsonResponse = JsonNode.Parse(responseBody);
            var missionJson = jsonResponse?["choices"]?[0]?["message"]?["content"]?.GetValue<string>();
            return missionJson ?? "{}";
        }
        catch (JsonException ex)
        {
            throw new JsonException("Failed to parse Groq API response.", ex);
        }
    }
}