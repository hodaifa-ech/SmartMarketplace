namespace SmartMarketplace.Web.Services;

public interface IGroqService
{
    Task<string> GenerateMissionJsonAsync(string userPrompt);
}
