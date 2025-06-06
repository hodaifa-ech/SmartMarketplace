using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace SmartMarketplace.Web.Models;

public class Mission
{
    [Key]
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    [Column(TypeName = "TEXT")]
    public string Description { get; set; } = string.Empty;
    
    public string Country { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string WorkMode { get; set; } = string.Empty; // REMOTE, ONSITE, HYBRID
    public int Duration { get; set; }
    public string DurationType { get; set; } = string.Empty; // MONTH, YEAR
    public bool StartImmediately { get; set; }
    public DateOnly? StartDate { get; set; }
    public string ExperienceYear { get; set; } = string.Empty; // 0-3, 3-7, 7-12, 12+
    public string ContractType { get; set; } = string.Empty; // FORFAIT, REGIE
    public int? EstimatedDailyRate { get; set; }
    public string Domain { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public string RequiredExpertises { get; set; } = string.Empty; // Stored as a comma-separated string
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}