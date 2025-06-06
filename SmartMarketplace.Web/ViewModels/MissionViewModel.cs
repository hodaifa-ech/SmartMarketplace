using System.Text.Json.Serialization;

namespace SmartMarketplace.Web.ViewModels;

// This class perfectly matches the JSON structure we expect from the AI
public class MissionViewModel
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("country")]
    public string Country { get; set; } = string.Empty;

    [JsonPropertyName("city")]
    public string City { get; set; } = string.Empty;

    [JsonPropertyName("workMode")]
    public string WorkMode { get; set; } = string.Empty;

    [JsonPropertyName("duration")]
    public int Duration { get; set; }

    [JsonPropertyName("durationType")]
    public string DurationType { get; set; } = string.Empty;

    [JsonPropertyName("startImmediately")]
    public bool StartImmediately { get; set; }

    [JsonPropertyName("startDate")]
    public string? StartDate { get; set; }

    [JsonPropertyName("experienceYear")]
    public string ExperienceYear { get; set; } = string.Empty;

    [JsonPropertyName("contractType")]
    public string ContractType { get; set; } = string.Empty;

    [JsonPropertyName("estimatedDailyRate")]
    public int? EstimatedDailyRate { get; set; }

    [JsonPropertyName("domain")]
    public string Domain { get; set; } = string.Empty;

    [JsonPropertyName("position")]
    public string Position { get; set; } = string.Empty;

    [JsonPropertyName("requiredExpertises")]
    public List<string> RequiredExpertises { get; set; } = [];
}