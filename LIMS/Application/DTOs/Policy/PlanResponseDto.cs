namespace Application.DTOs.Policy;

// What we send back — TenureOptions as a clean List, not raw string
public class PlanResponseDto
{
    public int PlanId { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal MinSumAssured { get; set; }
    public decimal MaxSumAssured { get; set; }
    public List<int> TenureOptions { get; set; } = new();  // parsed from "10,15,20"
    public string AvailableRiders { get; set; } = string.Empty;
    public int MinEntryAge { get; set; }
    public int MaxEntryAge { get; set; }
    public decimal MinAnnualIncome { get; set; }
    public decimal BaseRatePer1000 { get; set; }
    public decimal LowRiskMultiplier { get; set; }
    public decimal MediumRiskMultiplier { get; set; }
    public decimal HighRiskMultiplier { get; set; }
    public decimal CommissionPercentage { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public int TotalPoliciesCount { get; set; }  // useful for Admin dashboard
}