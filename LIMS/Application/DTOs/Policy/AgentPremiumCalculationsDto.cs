using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Policy;

// Agent only assigns risk category and optional remarks
// Customer already sent age/income/occupation at enrollment
public class AgentPremiumCalculationDto
{
    [Required]
    [RegularExpression("^(Low|Medium|Standard|High)$",
        ErrorMessage = "Risk category must be Low, Medium, Standard, or High")]
    public string RiskCategory { get; set; } = string.Empty;

    [StringLength(500)]
    public string? AgentRemarks { get; set; }
}