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

        // Act
        var result = await _service.GetAdminDashboardAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1000m, result.TotalPremiumCollected);
    }
}
