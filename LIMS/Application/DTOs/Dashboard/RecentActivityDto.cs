namespace Application.DTOs.Dashboard;

public class RecentActivityDto
{
    public string Type { get; set; } = string.Empty;    // "Policy", "Claim", "Payment"
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime Date { get; set; }
}