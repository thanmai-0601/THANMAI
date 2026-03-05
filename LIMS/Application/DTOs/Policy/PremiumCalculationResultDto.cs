namespace Application.DTOs.Policy;

// What we return after calculating premium — shown to Agent to explain to customer
public class PremiumCalculationResultDto
{
    public decimal SumAssured { get; set; }
    public int TenureYears { get; set; }
    public string RiskCategory { get; set; } = string.Empty;
    public decimal BaseRatePer1000 { get; set; }
    public decimal RiskMultiplierApplied { get; set; }
    public decimal AnnualPremium { get; set; }
    public decimal MonthlyPremium { get; set; }
    public decimal QuarterlyPremium { get; set; }
    public decimal TotalPremiumOverTenure { get; set; }
    public decimal CommissionAmount { get; set; }      // Agent's commission
    public decimal CommissionPercentage { get; set; }
}