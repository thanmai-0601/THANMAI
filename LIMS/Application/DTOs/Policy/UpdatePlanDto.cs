using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Application.DTOs.Policy;

// Same fields as Create — Admin can update anything
public class UpdatePlanDto
{
    [Required]
    [StringLength(150, MinimumLength = 3)]
    public string PlanName { get; set; } = string.Empty;

    [Required]
    [StringLength(1000, MinimumLength = 10)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public PlanType PlanType { get; set; } = PlanType.TermLife;

    [Range(0, 10)]
    public decimal BonusRatePerYear { get; set; }

    [Range(0, 100)]
    public int CoverageToAge { get; set; }

    [Required]
    [Range(100000, double.MaxValue)]
    public decimal MinSumAssured { get; set; }

    [Required]
    [Range(100000, double.MaxValue)]
    public decimal MaxSumAssured { get; set; }

    [Required]
    [MinLength(1)]
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
    [Range(1, 30)]
    public decimal CommissionPercentage { get; set; }

    public bool IsActive { get; set; } = true; // Admin can deactivate a plan
}