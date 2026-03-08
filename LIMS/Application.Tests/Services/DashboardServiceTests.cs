using Application.Interfaces.Repositories;
using Application.Services;
using Domain.Entities;
using Domain.Enums;
using Application.DTOs.Dashboard;
using Moq;
using Xunit;

namespace Application.Tests.Services;

public class DashboardServiceTests
{
    private readonly Mock<IPolicyRepository> _policyRepo;
    private readonly Mock<IClaimRepository> _claimRepo;
    private readonly Mock<IUserRepository> _userRepo;
    private readonly Mock<IPaymentRepository> _paymentRepo;
    private readonly Mock<ICommissionRepository> _commissionRepo;
    private readonly Mock<IInvoiceRepository> _invoiceRepo;
    private readonly Mock<IEndorsementRepository> _endorsementRepo;
    private readonly DashboardService _service;

    public DashboardServiceTests()
    {
        _policyRepo = new Mock<IPolicyRepository>();
        _claimRepo = new Mock<IClaimRepository>();
        _userRepo = new Mock<IUserRepository>();
        _paymentRepo = new Mock<IPaymentRepository>();
        _commissionRepo = new Mock<ICommissionRepository>();
        _invoiceRepo = new Mock<IInvoiceRepository>();
        _endorsementRepo = new Mock<IEndorsementRepository>();

        _service = new DashboardService(
            _policyRepo.Object, _claimRepo.Object, _userRepo.Object,
            _paymentRepo.Object, _commissionRepo.Object, _invoiceRepo.Object,
            _endorsementRepo.Object);
    }

    [Fact]
    public async Task GetAdminDashboardAsync_ShouldAggregateStats()
    {
        // Arrange
        _policyRepo.Setup(r => r.GetPolicyStatusCountsAsync()).ReturnsAsync(new List<PolicyStatusCountDto>());
        _claimRepo.Setup(r => r.GetClaimStatusCountsAsync()).ReturnsAsync(new List<ClaimStatusCountDto>());
        _userRepo.Setup(r => r.GetUserRoleCountsAsync()).ReturnsAsync(new List<UserRoleCountDto>());
        _paymentRepo.Setup(r => r.GetTotalPremiumCollectedAsync()).ReturnsAsync(1000m);
        _commissionRepo.Setup(r => r.GetTotalCommissionAsync()).ReturnsAsync(0m);
        _commissionRepo.Setup(r => r.GetTotalPendingCommissionAsync()).ReturnsAsync(0m);
        _claimRepo.Setup(r => r.GetTotalSettledAmountAsync()).ReturnsAsync(0m);
        _paymentRepo.Setup(r => r.GetLast12MonthsRevenueAsync()).ReturnsAsync(new List<MonthlyRevenueDto>());
        _policyRepo.Setup(r => r.GetAgentPerformanceAsync()).ReturnsAsync(new List<AgentPerformanceDto>());
        _policyRepo.Setup(r => r.GetPlanDistributionAsync()).ReturnsAsync(new List<PlanDistributionDto>());
        _endorsementRepo.Setup(r => r.GetTotalCountAsync()).ReturnsAsync(0);
        _endorsementRepo.Setup(r => r.GetPendingCountAsync()).ReturnsAsync(0);
        _paymentRepo.Setup(r => r.GetRecentAsync(It.IsAny<int>())).ReturnsAsync(new List<Domain.Entities.Payment>());

        // Act
        var result = await _service.GetAdminDashboardAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1000m, result.TotalPremiumCollected);
    }

    [Fact]
    public async Task GetCustomerDashboardAsync_ShouldExcludeSettledClaimsFromActiveCount()
    {
        // Arrange
        var customerId = 1;
        var policies = new List<Policy>
        {
            new Policy { 
                Id = 1,
                Status = PolicyStatus.Active, 
                Claims = new List<Claim> { new Claim { Status = ClaimStatus.Settled } } 
            },
            new Policy { Id = 2, Status = PolicyStatus.Active, Claims = new List<Claim>() },
            new Policy { Id = 3, Status = PolicyStatus.Submitted, Claims = new List<Claim>() }
        };

        _policyRepo.Setup(r => r.GetByCustomerIdAsync(customerId)).ReturnsAsync(policies);
        _claimRepo.Setup(r => r.GetClaimStatusCountsByCustomerAsync(customerId)).ReturnsAsync(new List<ClaimStatusCountDto>());
        _paymentRepo.Setup(r => r.GetTotalPaidByCustomerAsync(customerId)).ReturnsAsync(0m);
        _invoiceRepo.Setup(r => r.GetOutstandingAmountByCustomerAsync(customerId)).ReturnsAsync(0m);
        _invoiceRepo.Setup(r => r.GetOverdueCountByCustomerAsync(customerId)).ReturnsAsync(0);
        _invoiceRepo.Setup(r => r.GetUpcomingCountByCustomerAsync(customerId)).ReturnsAsync(0);
        _endorsementRepo.Setup(r => r.GetPendingByCustomerAsync(customerId)).ReturnsAsync(0);
        _policyRepo.Setup(r => r.GetRecentActivityByCustomerAsync(customerId)).ReturnsAsync(new List<RecentActivityDto>());
        _paymentRepo.Setup(r => r.GetRecentByCustomerAsync(customerId, 5)).ReturnsAsync(new List<Domain.Entities.Payment>());
        _claimRepo.Setup(r => r.GetTotalSettledByCustomerAsync(customerId)).ReturnsAsync(0m);

        // Act
        var result = await _service.GetCustomerDashboardAsync(customerId);

        // Assert
        Assert.Equal(3, result.TotalPolicies);
        Assert.Equal(1, result.ActivePolicies); // Only policy #2 is active (no settled claim)
        Assert.Equal(1, result.PendingPolicies); // Only policy #3 is pending (Submitted)
    }
}
