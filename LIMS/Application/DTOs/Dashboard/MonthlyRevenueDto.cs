namespace Application.DTOs.Dashboard;

public class MonthlyRevenueDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public decimal PremiumCollected { get; set; }
    public decimal CommissionPaid { get; set; }
    public int PoliciesActivated { get; set; }
}