using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Policy;

// Agent fills this after evaluating the customer
public class EligibilityDetailsDto
{
    [Required]
    [Range(18, 70, ErrorMessage = "Age must be between 18 and 70")]
    public int CustomerAge { get; set; }

    [Required]
    [Range(1, double.MaxValue, ErrorMessage = "Annual income must be greater than 0")]
    public decimal AnnualIncome { get; set; }

    [Required]
    [StringLength(100)]
    public string Occupation { get; set; } = string.Empty;

    [Required]
    [RegularExpression("^(Low|Medium|High)$",
        ErrorMessage = "Risk category must be Low, Medium, or High")]
    public string RiskCategory { get; set; } = string.Empty;

    [StringLength(500)]
    public string? AgentRemarks { get; set; }
}