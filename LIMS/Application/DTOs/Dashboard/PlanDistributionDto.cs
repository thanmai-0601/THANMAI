namespace Application.DTOs.Dashboard;

public class PlanDistributionDto
{
    public int PlanId { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public int TotalPolicies { get; set; }
    public int ActivePolicies { get; set; }
    public decimal TotalSumAssured { get; set; }
    public decimal TotalPremiumCollected { get; set; }
}