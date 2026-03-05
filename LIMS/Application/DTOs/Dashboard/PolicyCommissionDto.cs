namespace Application.DTOs.Dashboard;

public class PolicyCommissionDto
{
    public string PolicyNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public decimal PremiumAmount { get; set; }
    public decimal CommissionAmount { get; set; }
    public decimal CommissionPercentage { get; set; }
    public DateTime EarnedOn { get; set; }
}