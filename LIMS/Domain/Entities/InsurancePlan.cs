using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;

public class InsurancePlan : BaseEntity
{
    public string PlanName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    // Plan category: TermLife, WholeLife, or Endowment
    public PlanType PlanType { get; set; } = PlanType.TermLife;

    // Endowment-specific: annual bonus accrual rate (e.g. 2.5 means 2.5% of SumAssured per year)
    public decimal BonusRatePerYear { get; set; }

    // WholeLife-specific: coverage continues until this age (e.g. 99)
    public int CoverageToAge { get; set; }

    // Sum Assured Range (how much coverage the plan offers)
    public decimal MinSumAssured { get; set; }
    public decimal MaxSumAssured { get; set; }

    // Tenure Options stored as comma-separated years e.g. "10,15,20,25,30"
    // Simple approach — no extra table needed for a list of numbers
    public string TenureOptions { get; set; } = string.Empty;

    // Eligibility Rules
    public int MinEntryAge { get; set; }
    public int MaxEntryAge { get; set; }
    public decimal MinAnnualIncome { get; set; }


    // Premium Calculation Factors (base rate per 1000 sum assured)
    // Agent will use these to dynamically calculate premium
    public decimal BaseRatePer1000 { get; set; }

    // Risk multipliers — Agent picks the category during evaluation
    public decimal LowRiskMultiplier { get; set; }    // e.g. 1.0
    public decimal MediumRiskMultiplier { get; set; } // e.g. 1.25
    public decimal HighRiskMultiplier { get; set; }   // e.g. 1.6

    // Commission % for Agent (used by CommissionService later)
    public decimal CommissionPercentage { get; set; }

    public bool IsActive { get; set; } = true;  // Admin can deactivate without deleting

    // Navigation — one plan can have many policies
    public ICollection<Policy> Policies { get; set; } = new List<Policy>();
}