namespace Application.DTOs.Dashboard;

public class ClaimsOfficerDashboardDto
{
    public int TotalAssignedClaims { get; set; }
    public int SubmittedClaims { get; set; }
    public int UnderReviewClaims { get; set; }
    public int SettledClaims { get; set; }
    public int RejectedClaims { get; set; }
    public decimal TotalSettledAmount { get; set; }
    public decimal ThisMonthSettledAmount { get; set; }
}