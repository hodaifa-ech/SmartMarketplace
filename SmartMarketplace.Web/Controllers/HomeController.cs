using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Assurez-vous d'avoir cet using
using SmartMarketplace.Web.Data;
using SmartMarketplace.Web.Models;
using SmartMarketplace.Web.Services;
using SmartMarketplace.Web.ViewModels;

namespace SmartMarketplace.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IGroqService _groqService;
    private readonly ApplicationDbContext _context;

    public HomeController(ILogger<HomeController> logger, IGroqService groqService, ApplicationDbContext context)
    {
        _logger = logger;
        _groqService = groqService;
        _context = context;
    }

    public IActionResult Index()
    {
        return View();
    }
    
    // NOUVELLE ACTION POUR L'HISTORIQUE
    public async Task<IActionResult> History()
    {
        var missions = await _context.Missions
                                     .OrderByDescending(m => m.CreatedAt)
                                     .ToListAsync();
        return View(missions);
    }


   [HttpPost]
[Route("api/generate-mission")]
public async Task<IActionResult> GenerateMission([FromBody] PromptRequest request) // PromptRequest ne contient plus que le prompt
{
    if (string.IsNullOrWhiteSpace(request.Prompt))
    {
        return BadRequest(new { message = "Prompt cannot be empty." });
    }

    try
    {
        // 1. Appel simplifié au service Groq
        var missionJson = await _groqService.GenerateMissionJsonAsync(request.Prompt);

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var viewModel = JsonSerializer.Deserialize<MissionViewModel>(missionJson, options);

        if (viewModel == null)
        {
            return StatusCode(500, new { message = "Failed to deserialize AI response." });
        }
        
        var mission = new Mission
        {
            Title = viewModel.Title,
            Description = viewModel.Description,
            Country = viewModel.Country,
            City = viewModel.City,
            WorkMode = viewModel.WorkMode,
            Duration = viewModel.Duration,
            DurationType = viewModel.DurationType,
            StartImmediately = viewModel.StartImmediately,
            StartDate = viewModel.StartImmediately || string.IsNullOrEmpty(viewModel.StartDate) 
                ? null 
                : DateOnly.Parse(viewModel.StartDate),
            ExperienceYear = viewModel.ExperienceYear,
            ContractType = viewModel.ContractType,
            EstimatedDailyRate = viewModel.EstimatedDailyRate,
            Domain = viewModel.Domain,
            Position = viewModel.Position,
            RequiredExpertises = string.Join(", ", viewModel.RequiredExpertises),
            // 2. La langue est maintenant toujours en "Auto" car l'IA gère tout
            GeneratedLanguage = "Auto-detected",
            CreatedAt = DateTime.UtcNow
        };

        _context.Missions.Add(mission);
        await _context.SaveChangesAsync();

        return Json(viewModel);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "An error occurred while generating the mission.");
        return StatusCode(500, new { message = $"An internal error occurred: {ex.Message}" });
    }
}

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}