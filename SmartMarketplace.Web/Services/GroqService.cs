using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace SmartMarketplace.Web.Services;

public interface IGroqService
{
    Task<string> GenerateMissionJsonAsync(string userPrompt);
}

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

    public async Task<string> GenerateMissionJsonAsync(string userPrompt)
    {
        var httpClient = _httpClientFactory.CreateClient("Groq");
        
        // The detailed system prompt that forces the AI to respond in the desired JSON format.
        var systemPrompt = @"
Tu es un assistant spécialisé dans la création de fiches mission pour une marketplace de freelances.
À partir d'une brève description, tu dois générer une fiche mission complète.
Réponds UNIQUEMENT au format JSON suivant (sans commentaires, sans texte avant ou après le JSON).

{
  ""title"": ""Titre concis et accrocheur de la mission"",
  ""description"": ""Description détaillée incluant contexte et responsabilités. Utilise des retours à la ligne avec \n pour la mise en forme."",
  ""country"": ""Nom du pays en anglais"",
  ""city"": ""Nom de la ville en anglais"",
  ""workMode"": ""Un parmi: REMOTE, ONSITE, HYBRID"",
  ""duration"": ""Durée (nombre)"",
  ""durationType"": ""Unité de durée (MONTH, YEAR)"",
  ""startImmediately"": true/false,
  ""startDate"": ""Date de début au format yyyy-MM-dd (seulement si startImmediately est false, sinon null)"",
  ""experienceYear"": ""Un parmi: 0-3, 3-7, 7-12, 12+"",
  ""contractType"": ""Un parmi: FORFAIT, REGIE"",
  ""estimatedDailyRate"": ""Taux journalier moyen en euros (nombre uniquement, sans le symbole €)"",
  ""domain"": ""Domaine d'activité principal (ex: Technology, Finance, Marketing)"",
  ""position"": ""Intitulé du poste/fonction (ex: React Developer, Project Manager)"",
  ""requiredExpertises"": [""expertise1"", ""expertise2"", ""...""]
}
";

        var requestPayload = new
        {
            messages = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userPrompt }
            },
            model = "llama3-70b-8192", // Using the powerful Llama 3 70b model
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
        
        // Safely parse the JSON and extract the content
        try
        {
            var jsonResponse = JsonNode.Parse(responseBody);
            var missionJson = jsonResponse?["choices"]?[0]?["message"]?["content"]?.GetValue<string>();
            return missionJson ?? "{}"; // Return empty JSON if parsing fails
        }
        catch (JsonException ex)
        {
            throw new JsonException("Failed to parse Groq API response.", ex);
        }
    }
}