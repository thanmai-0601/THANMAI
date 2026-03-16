using Application.DTOs.Dashboard;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Enums;

namespace Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IPolicyRepository _policyRepo;
    private readonly IClaimRepository _claimRepo;
    private readonly IUserRepository _userRepo;
    private readonly IPaymentRepository _paymentRepo;
    private readonly ICommissionRepository _commissionRepo;
    private readonly IInvoiceRepository _invoiceRepo;
    private readonly IEndorsementRepository _endorsementRepo;

    public DashboardService(
        IPolicyRepository policyRepo,
        IClaimRepository claimRepo,
        IUserRepository userRepo,
        IPaymentRepository paymentRepo,
        ICommissionRepository commissionRepo,
        IInvoiceRepository invoiceRepo,
        IEndorsementRepository endorsementRepo)
    {
        _policyRepo = policyRepo;
        _claimRepo = claimRepo;
        _userRepo = userRepo;
        _paymentRepo = paymentRepo;
        _commissionRepo = commissionRepo;
        _invoiceRepo = invoiceRepo;
        _endorsementRepo = endorsementRepo;
    }

    // ───────────────── ADMIN DASHBOARD ─────────────────

    public async Task<AdminDashboardDto> GetAdminDashboardAsync(int? year = null)
    {
        var policyStats = await _policyRepo.GetPolicyStatusCountsAsync();
        var claimStats = await _claimRepo.GetClaimStatusCountsAsync();
        var userStats = await _userRepo.GetUserRoleCountsAsync();

        var totalRevenue = await _paymentRepo.GetTotalPremiumCollectedAsync();
        var totalCommission = await _commissionRepo.GetTotalCommissionAsync();
        var totalPendingCommission = await _commissionRepo.GetTotalPendingCommissionAsync();
        var totalSettledAmount = await _claimRepo.GetTotalSettledAmountAsync();

        var selectedYear = year ?? DateTime.UtcNow.Year;
        var rawMonthlyRevenue = await _paymentRepo.GetRevenueByYearAsync(selectedYear);
        var monthlyRevenue = new List<MonthlyRevenueDto>();
        
        // Show full year (Jan to Dec) for that year
        for (int m = 1; m <= 12; m++)
        {
            var date = new DateTime(selectedYear, m, 1);
            var existing = rawMonthlyRevenue.FirstOrDefault(x => x.Year == date.Year && x.Month == date.Month);
            if (existing != null)
            {
                monthlyRevenue.Add(existing);
            }
            else
            {
                monthlyRevenue.Add(new MonthlyRevenueDto
                {
                    Year = date.Year,
                    Month = date.Month,
                    MonthName = date.ToString("MMM yyyy"),
                    PremiumCollected = 0,
                    PoliciesActivated = 0
                });
            }
        }

        var agentPerformance = await _policyRepo.GetAgentPerformanceAsync();
        var planDistribution = await _policyRepo.GetPlanDistributionAsync();

        var totalEndorsements = await _endorsementRepo.GetTotalCountAsync();
        var pendingEndorsements =
            await _endorsementRepo.GetPendingCountAsync();

        var recentPayments = await _paymentRepo.GetRecentAsync(10);

        return new AdminDashboardDto
        {
            // Policies
            TotalPolicies = policyStats.Sum(p => p.Count),
            ActivePolicies = planDistribution.Sum(p => p.ActivePolicies),
            SubmittedPolicies = policyStats
                .FirstOrDefault(p => p.Status == PolicyStatus.Submitted)?.Count ?? 0,
            UnderReviewPolicies = policyStats
                .FirstOrDefault(p => p.Status == PolicyStatus.UnderReview)?.Count ?? 0,
            RejectedPolicies = policyStats
                .FirstOrDefault(p => p.Status == PolicyStatus.Rejected)?.Count ?? 0,
            SuspendedPolicies = policyStats
                .FirstOrDefault(p => p.Status == PolicyStatus.Suspended)?.Count ?? 0,
            LapsedPolicies = policyStats
                .FirstOrDefault(p => p.Status == PolicyStatus.Lapsed)?.Count ?? 0,
            SettledPolicies = (policyStats.FirstOrDefault(p => p.Status == PolicyStatus.Settled)?.Count ?? 0)
                              + await _policyRepo.GetActiveWithSettledClaimCountAsync(),

            // Claims
            TotalClaims = claimStats.Sum(c => c.Count),
            SubmittedClaims = claimStats
                .FirstOrDefault(c => c.Status == ClaimStatus.Submitted)?.Count ?? 0,
            UnderReviewClaims = (claimStats.FirstOrDefault(c => c.Status == ClaimStatus.Submitted)?.Count ?? 0) +
                                (claimStats.FirstOrDefault(c => c.Status == ClaimStatus.UnderReview)?.Count ?? 0),
            SettledClaims = (claimStats.FirstOrDefault(c => c.Status == ClaimStatus.Approved)?.Count ?? 0) +
                            (claimStats.FirstOrDefault(c => c.Status == ClaimStatus.Settled)?.Count ?? 0),
            RejectedClaims = claimStats
                .FirstOrDefault(c => c.Status == ClaimStatus.Rejected)?.Count ?? 0,
            TotalSettledAmount = totalSettledAmount,

            // Revenue
            TotalPremiumCollected = totalRevenue,
            TotalCommissionPaid = totalCommission,
            TotalPendingCommission = totalPendingCommission,
            MonthlyRevenue = monthlyRevenue,

            // Users
            TotalCustomers = userStats
                .FirstOrDefault(u => u.Role == UserRole.Customer)?.Count ?? 0,
            TotalAgents = userStats
                .FirstOrDefault(u => u.Role == UserRole.Agent)?.Count ?? 0,
            TotalClaimsOfficers = userStats
                .FirstOrDefault(u => u.Role == UserRole.ClaimsOfficer)?.Count ?? 0,

            // Performance
            AgentPerformance = agentPerformance,
            PlanDistribution = planDistribution,

            // Endorsements
            TotalEndorsements = totalEndorsements,
            PendingEndorsements = pendingEndorsements,

            // Payments
            RecentPayments = recentPayments.Select(p => new Application.DTOs.Payment.PaymentResponseDto
            {
                PaymentId = p.Id,
                PolicyNumber = p.Policy?.PolicyNumber ?? "N/A",
                CustomerName = p.Policy?.Customer?.FullName,
                AmountPaid = p.AmountPaid,
                Status = p.Status.ToString(),
                PaymentMethod = p.PaymentMethod.ToString(),
                TransactionReference = p.TransactionReference,
                PaymentDate = p.PaymentDate
            }).ToList()
        };
    }

    // ───────────────── AGENT DASHBOARD ─────────────────

    public async Task<AgentDashboardDto> GetAgentDashboardAsync(int agentId)
    {
        var policyStats =
            await _policyRepo.GetPolicyStatusCountsByAgentAsync(agentId);

        var totalCommission =
            await _commissionRepo.GetTotalByAgentAsync(agentId);

        var pendingCommission =
            await _commissionRepo.GetPendingTotalByAgentAsync(agentId);

        var thisMonthCommission =
            await _commissionRepo.GetThisMonthByAgentAsync(agentId);

        var lastMonthCommission =
            await _commissionRepo.GetLastMonthByAgentAsync(agentId);

        var recentCommissions =
            await _commissionRepo.GetRecentByAgentAsync(agentId);

        var pendingEndorsements =
            await _endorsementRepo.GetPendingByAgentAsync(agentId);

        return new AgentDashboardDto
        {
            TotalAssignedPolicies = policyStats.Sum(p => p.Count),
            SubmittedPolicies = policyStats
                .FirstOrDefault(p => p.Status == PolicyStatus.Submitted)?.Count ?? 0,
            UnderReviewPolicies = policyStats
                .FirstOrDefault(p => p.Status == PolicyStatus.UnderReview)?.Count ?? 0,
            ActivePolicies = policyStats
                .FirstOrDefault(p => p.Status == PolicyStatus.Active)?.Count ?? 0,
            RejectedPolicies = policyStats
                .FirstOrDefault(p => p.Status == PolicyStatus.Rejected)?.Count ?? 0,
            SettledPolicies = policyStats
                .FirstOrDefault(p => p.Status == PolicyStatus.Settled)?.Count ?? 0,

            TotalCommissionEarned = totalCommission,
            PendingCommission = pendingCommission,
            ThisMonthCommission = thisMonthCommission,
            LastMonthCommission = lastMonthCommission,
            RecentCommissions = recentCommissions,
            PendingEndorsements = pendingEndorsements.Select(e => new Application.DTOs.Endorsement.EndorsementResponseDto
            {
                EndorsementId = e.Id,
                PolicyId = e.PolicyId,
                PolicyNumber = e.Policy?.PolicyNumber ?? string.Empty,
                Type = e.Type.ToString(),
                Status = e.Status.ToString(),
                ChangeRequested = e.ChangeRequestJson,
                OldValue = e.OldValueJson,
                CustomerName = e.RequestedByCustomer?.FullName ?? string.Empty,
                AgentName = e.ReviewedByAgent?.FullName,
                AgentRemarks = e.AgentRemarks,
                RejectionReason = e.RejectionReason,
                RequestedAt = e.RequestedAt,
                ReviewedAt = e.ReviewedAt,
                ApprovedAt = e.ApprovedAt
            }).ToList()
        };
    }

    // ───────────────── CUSTOMER DASHBOARD ─────────────────

    public async Task<CustomerDashboardDto> GetCustomerDashboardAsync(
        int customerId)
    {
        var policies = await _policyRepo.GetByCustomerIdAsync(customerId);
        
        var claimStats =
            await _claimRepo.GetClaimStatusCountsByCustomerAsync(customerId);

        var totalPaid =
            await _paymentRepo.GetTotalPaidByCustomerAsync(customerId);

        var totalOutstanding =
            await _invoiceRepo.GetOutstandingAmountByCustomerAsync(customerId);

        var overdueInvoices =
            await _invoiceRepo.GetOverdueCountByCustomerAsync(customerId);

        var upcomingInvoices =
            await _invoiceRepo.GetUpcomingCountByCustomerAsync(customerId);

        var pendingEndorsements =
            await _endorsementRepo.GetPendingByCustomerAsync(customerId);

        var recentActivity =
            await _policyRepo.GetRecentActivityByCustomerAsync(customerId);

        var recentPayments = await _paymentRepo.GetRecentByCustomerAsync(customerId, 5);

        var customerClaims = await _claimRepo.GetByCustomerIdAsync(customerId);
        var hasSettledDeathClaim = customerClaims.Any(c => 
            c.Type == ClaimType.Death && 
            c.Status == ClaimStatus.Settled);

        return new CustomerDashboardDto
        {
            TotalPolicies = policies.Count,
            ActivePolicies = policies.Count(p => 
                p.Status == PolicyStatus.Active && 
                !p.Claims.Any(c => c.Status == ClaimStatus.Settled)),
            
            PendingPolicies = policies.Count(p => 
                p.Status == PolicyStatus.Submitted || 
                p.Status == PolicyStatus.UnderReview ||
                p.Status == PolicyStatus.Approved ||
                p.Status == PolicyStatus.DocumentsSubmitted),

            RejectedPolicies = policies.Count(p => p.Status == PolicyStatus.Rejected),

            TotalClaims = claimStats.Sum(c => c.Count),
            OpenClaims = claimStats
                .Where(c => c.Status == ClaimStatus.Submitted || c.Status == ClaimStatus.UnderReview || c.Status == ClaimStatus.Approved)
                .Sum(c => c.Count),
            SettledClaims = claimStats
                .FirstOrDefault(c => c.Status == ClaimStatus.Settled)?.Count ?? 0,
            TotalSettledAmount =
                await _claimRepo.GetTotalSettledByCustomerAsync(customerId),

            OverdueInvoices = overdueInvoices,
            UpcomingInvoices = upcomingInvoices,
            TotalPaidAmount = totalPaid,
            TotalOutstandingAmount = totalOutstanding,

            PendingEndorsements = pendingEndorsements,
            HasSettledDeathClaim = hasSettledDeathClaim, 
            RecentActivity = recentActivity,
            RecentPayments = recentPayments.Select(p => new Application.DTOs.Payment.PaymentResponseDto
            {
                PaymentId = p.Id,
                PolicyNumber = p.Policy?.PolicyNumber ?? "N/A",
                CustomerName = p.Policy?.Customer?.FullName,
                AmountPaid = p.AmountPaid,
                Status = p.Status.ToString(),
                PaymentMethod = p.PaymentMethod.ToString(),
                TransactionReference = p.TransactionReference,
                PaymentDate = p.PaymentDate
            }).ToList()
        };
    }

    // ───────────────── CLAIMS OFFICER DASHBOARD ─────────────────

    public async Task<ClaimsOfficerDashboardDto>
        GetClaimsOfficerDashboardAsync(int officerId)
    {
        var claimStats =
            await _claimRepo.GetClaimStatusCountsByOfficerAsync(officerId);

        var totalSettled =
            await _claimRepo.GetTotalSettledByOfficerAsync(officerId);

        var thisMonthSettled =
            await _claimRepo.GetThisMonthSettledByOfficerAsync(officerId);

        return new ClaimsOfficerDashboardDto
        {
            TotalAssignedClaims = claimStats.Sum(c => c.Count),
            
            SubmittedClaims = claimStats
                .FirstOrDefault(c => c.Status == ClaimStatus.Submitted)?.Count ?? 0,

            // Combine Submitted + UnderReview as "Under Review" for the dashboard
            UnderReviewClaims = (claimStats.FirstOrDefault(c => c.Status == ClaimStatus.Submitted)?.Count ?? 0) +
                                (claimStats.FirstOrDefault(c => c.Status == ClaimStatus.UnderReview)?.Count ?? 0),
            
            // Combine Approved + Settled as "Approved Claims"
            SettledClaims = (claimStats.FirstOrDefault(c => c.Status == ClaimStatus.Approved)?.Count ?? 0) +
                            (claimStats.FirstOrDefault(c => c.Status == ClaimStatus.Settled)?.Count ?? 0),
            
            RejectedClaims = claimStats
                .FirstOrDefault(c => c.Status == ClaimStatus.Rejected)?.Count ?? 0,

            TotalSettledAmount = totalSettled,
            ThisMonthSettledAmount = thisMonthSettled
        };
    }
}