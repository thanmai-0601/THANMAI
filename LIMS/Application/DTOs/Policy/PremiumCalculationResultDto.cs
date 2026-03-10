using Domain.Enums;

namespace Application.DTOs.Policy;

// What we return after calculating premium — shown to Agent to explain to customer
public class PremiumCalculationResultDto
{
    public PlanType PlanType { get; set; }
    public decimal SumAssured { get; set; }
    public int TenureYears { get; set; }
    public string RiskCategory { get; set; } = string.Empty;
    public decimal BaseRatePer1000 { get; set; }
    public decimal RiskMultiplierApplied { get; set; }
    public decimal AnnualPremium { get; set; }
    public decimal TotalPremiumOverTenure { get; set; }
    public decimal CommissionAmount { get; set; }      // Agent's commission
    public decimal CommissionPercentage { get; set; }

    // Endowment-specific: what the policyholder gets on survival
    public decimal EstimatedBonus { get; set; }        // SumAssured × BonusRate × Tenure
    public decimal MaturityBenefit { get; set; }       // SumAssured + EstimatedBonus
}