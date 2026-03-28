namespace Application.DTOs.Dashboard;

public class CustomerDistributionDto
{
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public int TotalPolicies { get; set; }
    public int TotalClaims { get; set; }
}
