namespace Application.DTOs.Dashboard;

public class CustomerDashboardDto
{
    // ── Policy Summary ────────────────────────────────────────
    public int TotalPolicies { get; set; }
    public int ActivePolicies { get; set; }
    public int PendingPolicies { get; set; }  // Submitted + UnderReview
    public int RejectedPolicies { get; set; }

    // ── Claims Summary ────────────────────────────────────────
    public int TotalClaims { get; set; }
    public int OpenClaims { get; set; }
    public int SettledClaims { get; set; }
    public decimal TotalSettledAmount { get; set; }

    // ── Payments Summary ──────────────────────────────────────
    public int OverdueInvoices { get; set; }
    public int UpcomingInvoices { get; set; }   // due in next 30 days
    public decimal TotalPaidAmount { get; set; }
    public decimal TotalOutstandingAmount { get; set; }

    // ── Endorsements ──────────────────────────────────────────
    public int PendingEndorsements { get; set; }

    // ── Recent Activity ───────────────────────────────────────
    public List<RecentActivityDto> RecentActivity { get; set; } = new();
    public List<Application.DTOs.Payment.PaymentResponseDto> RecentPayments { get; set; } = new();
}