using Domain.Entities;

using Application.DTOs.Endorsement;

namespace Application.DTOs.Dashboard;

public class AgentDashboardDto
{
    // ── Policy Summary ────────────────────────────────────────
    public int TotalAssignedPolicies { get; set; }
    public int SubmittedPolicies { get; set; }
    public int UnderReviewPolicies { get; set; }
    public int ActivePolicies { get; set; }
    public int RejectedPolicies { get; set; }
    public int SettledPolicies { get; set; }

    // ── Commission Summary ────────────────────────────────────
    public decimal TotalCommissionEarned { get; set; }
    public decimal PendingCommission { get; set; }
    public decimal ThisMonthCommission { get; set; }
    public decimal LastMonthCommission { get; set; }
    public List<PolicyCommissionDto> RecentCommissions { get; set; } = new();

    // ── Endorsements ──────────────────────────────────────────
    public List<EndorsementResponseDto> PendingEndorsements { get; set; } = new();
}