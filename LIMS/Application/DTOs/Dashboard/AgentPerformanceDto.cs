namespace Application.DTOs.Dashboard;

public class AgentPerformanceDto
{
    public int AgentId { get; set; }
    public string AgentName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int TotalPoliciesAssigned { get; set; }
    public int ActivePolicies { get; set; }
    public int ApprovedPolicies { get; set; }
    public int RejectedPolicies { get; set; }
    public decimal TotalCommissionEarned { get; set; }
    public decimal ConversionRate { get; set; }  // Approved / Total %
}