namespace Application.DTOs.Dashboard;

public class CustomerPolicyFinancialsDto
{
    public string PolicyNumber { get; set; } = string.Empty;
    public string PlanName { get; set; } = string.Empty;
    public decimal TotalPremiumPaid { get; set; }
    public decimal TotalClaimReceived { get; set; }
}
