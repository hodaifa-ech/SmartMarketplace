using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
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

    [HttpPost]
    [Route("api/generate-mission")]
    public async Task<IActionResult> GenerateMission([FromBody] PromptRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Prompt))
        {
            return BadRequest(new { message = "Prompt cannot be empty." });
        }

        try
        {
            // 1. Call Groq to get the mission details as a JSON string
            var missionJson = await _groqService.GenerateMissionJsonAsync(request.Prompt);

            // 2. Deserialize the JSON string into our ViewModel
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var viewModel = JsonSerializer.Deserialize<MissionViewModel>(missionJson, options);

            if (viewModel == null)
            {
                return StatusCode(500, new { message = "Failed to deserialize AI response." });
            }
            
            // 3. Map ViewModel to Database Entity
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
                // Store list as a comma-separated string
                RequiredExpertises = string.Join(", ", viewModel.RequiredExpertises),
                CreatedAt = DateTime.UtcNow
            };

            // 4. Save the generated mission to the database
            _context.Missions.Add(mission);
            await _context.SaveChangesAsync();

            // 5. Return the ViewModel to the front-end
            return Json(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while generating the mission.");
            return StatusCode(500, new { message = "An internal error occurred. Please try again." });
        }
    }


    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}