namespace Application.DTOs.Dashboard;

public class AdminDashboardDto
{
    // ── Policy Overview ───────────────────────────────────────
    public int TotalPolicies { get; set; }
    public int ActivePolicies { get; set; }
    public int SubmittedPolicies { get; set; }
    public int UnderReviewPolicies { get; set; }
    public int RejectedPolicies { get; set; }
    public int SuspendedPolicies { get; set; }
    public int LapsedPolicies { get; set; }
    public int SettledPolicies { get; set; }

    // ── Claims Overview ───────────────────────────────────────
    public int TotalClaims { get; set; }
    public int SubmittedClaims { get; set; }
    public int UnderReviewClaims { get; set; }
    public int SettledClaims { get; set; }
    public int RejectedClaims { get; set; }
    public decimal TotalSettledAmount { get; set; }

    // ── Revenue ───────────────────────────────────────────────
    public decimal TotalPremiumCollected { get; set; }
    public decimal TotalCommissionPaid { get; set; }
    public decimal TotalPendingCommission { get; set; }
    public List<MonthlyRevenueDto> MonthlyRevenue { get; set; } = new();

    // ── Users ─────────────────────────────────────────────────
    public int TotalCustomers { get; set; }
    public int TotalAgents { get; set; }
    public int TotalClaimsOfficers { get; set; }

    // ── Agent Performance ─────────────────────────────────────
    public List<AgentPerformanceDto> AgentPerformance { get; set; } = new();

    // ── Plan Distribution ─────────────────────────────────────
    public List<PlanDistributionDto> PlanDistribution { get; set; } = new();

    // ── Endorsements ──────────────────────────────────────────
    public int TotalEndorsements { get; set; }
    public int PendingEndorsements { get; set; }

    // ── Recent Payments ───────────────────────────────────────
    public List<Payment.PaymentResponseDto> RecentPayments { get; set; } = new();
}