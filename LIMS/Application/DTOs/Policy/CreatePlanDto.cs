using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Application.DTOs.Policy;

public class CreatePlanDto
{
    [Required]
    [StringLength(150, MinimumLength = 3)]
    public string PlanName { get; set; } = string.Empty;

    [Required]
    [StringLength(1000, MinimumLength = 10)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public PlanType PlanType { get; set; } = PlanType.TermLife;

    // Endowment-specific: annual bonus % (e.g. 2.5)
    [Range(0, 10)]
    public decimal BonusRatePerYear { get; set; }

    // WholeLife-specific: coverage until this age (e.g. 99)
    [Range(0, 100)]
    public int CoverageToAge { get; set; }

    [Required]
    [Range(100000, double.MaxValue, ErrorMessage = "Minimum sum assured must be at least 1,00,000")]
    public decimal MinSumAssured { get; set; }

    [Required]
    [Range(100000, double.MaxValue)]
    public decimal MaxSumAssured { get; set; }

    // List of valid tenure years e.g. [10, 15, 20, 25]
    [Required]
    [MinLength(1, ErrorMessage = "At least one tenure option is required")]
    public List<int> TenureOptions { get; set; } = new();


    [Required]
    [Range(18, 60)]
    public int MinEntryAge { get; set; }

    [Required]
    [Range(18, 70)]
    public int MaxEntryAge { get; set; }

    [Required]
    [Range(0, double.MaxValue)]
    public decimal MinAnnualIncome { get; set; }

    [Required]
    [Range(0.0001, double.MaxValue)]
    public decimal BaseRatePer1000 { get; set; }

    [Required]
    [Range(0.1, 5.0)]
    public decimal LowRiskMultiplier { get; set; }

    [Required]
    [Range(0.1, 5.0)]
    public decimal MediumRiskMultiplier { get; set; }

    [Required]
    [Range(0.1, 5.0)]
    public decimal HighRiskMultiplier { get; set; }

    [Required]
    [Range(1, 30, ErrorMessage = "Commission % must be between 1 and 30")]
    public decimal CommissionPercentage { get; set; }
}