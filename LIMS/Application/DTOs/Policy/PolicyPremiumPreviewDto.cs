namespace Application.DTOs.Policy;

// Shown to customer after agent calculates premium
public class PremiumPreviewDto
{
    public string PolicyNumber { get; set; } = string.Empty;
    public string PlanName { get; set; } = string.Empty;
    public decimal SumAssured { get; set; }
    public int TenureYears { get; set; }
    public string RiskCategory { get; set; } = string.Empty;

    // Premium breakdown
    public decimal AnnualPremium { get; set; }
    public decimal MonthlyPremium { get; set; }
    public decimal QuarterlyPremium { get; set; }
    public decimal TotalPayableOverTenure { get; set; }

    // Coverage details
    public decimal SumAssuredOnDeath { get; set; }      // = SumAssured
    public decimal SumAssuredOnMaturity { get; set; }   // = SumAssured (for simplicity)

    // Benefits (derived from plan rules)
    public List<string> Benefits { get; set; } = new();

    public string AgentRemarks { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}