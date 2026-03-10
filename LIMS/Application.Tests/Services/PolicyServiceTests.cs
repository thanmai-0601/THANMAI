using Application.DTOs.Policy;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Services;
using Domain.Entities;
using Domain.Enums;
using Moq;
using Xunit;

namespace Application.Tests.Services;

public class PolicyServiceTests
{
    private readonly Mock<IPolicyRepository> _policyRepoMock;
    private readonly Mock<IPlanRepository> _planRepoMock;
    private readonly Mock<IAgentAssignmentService> _agentAssignmentMock;
    private readonly Mock<INotificationService> _notificationMock;
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IClaimRepository> _claimRepoMock;
    private readonly PolicyService _policyService;

    public PolicyServiceTests()
    {
        _policyRepoMock = new Mock<IPolicyRepository>();
        _planRepoMock = new Mock<IPlanRepository>();
        _agentAssignmentMock = new Mock<IAgentAssignmentService>();
        _notificationMock = new Mock<INotificationService>();
        _userRepoMock = new Mock<IUserRepository>();
        _claimRepoMock = new Mock<IClaimRepository>();
        
        _policyService = new PolicyService(
            _policyRepoMock.Object,
            _planRepoMock.Object,
            _agentAssignmentMock.Object,
            _notificationMock.Object,
            _userRepoMock.Object,
            _claimRepoMock.Object);
    }

    [Fact]
    public async Task RequestPolicyAsync_ShouldSucceed_WhenValidRequest()
    {
        // Arrange
        var customerId = 1;
        var dto = new RequestPolicyDto 
        { 
            InsurancePlanId = 10, 
            SumAssured = 100000, 
            TenureYears = 10,
            AnnualIncome = 500000
        };

        var plan = new InsurancePlan 
        { 
            Id = 10, 
            IsActive = true, 
            MinSumAssured = 50000, 
            MaxSumAssured = 500000,
            TenureOptions = "10,20",
            MinEntryAge = 18,
            MaxEntryAge = 60,
            MinAnnualIncome = 100000
        };

        _planRepoMock.Setup(r => r.GetByIdAsync(dto.InsurancePlanId)).ReturnsAsync(plan);
        _claimRepoMock.Setup(r => r.GetByCustomerIdAsync(customerId)).ReturnsAsync(new List<Claim>());
        
        var user = new User { Id = customerId, DateOfBirth = new DateTime(1990, 1, 1) };
        _userRepoMock.Setup(r => r.GetByIdAsync(customerId)).ReturnsAsync(user);

        _agentAssignmentMock.Setup(a => a.AssignAgentAsync()).ReturnsAsync(5);
        _policyRepoMock.Setup(r => r.GeneratePolicyNumberAsync()).ReturnsAsync("POL-001");
        
        var policy = new Policy { Id = 1, PolicyNumber = "POL-001" };
        _policyRepoMock.Setup(r => r.GetByIdWithDetailsAsync(It.IsAny<int>())).ReturnsAsync(policy);

        // Act
        var result = await _policyService.RequestPolicyAsync(customerId, dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("POL-001", result.PolicyNumber);
        _policyRepoMock.Verify(r => r.CreateAsync(It.IsAny<Policy>()), Times.Once);
        _notificationMock.Verify(n => n.CreateNotificationAsync(customerId, It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task RequestPolicyAsync_ShouldThrow_WhenPlanInactive()
    {
        // Arrange
        var dto = new RequestPolicyDto { InsurancePlanId = 10 };
        var plan = new InsurancePlan { Id = 10, IsActive = false };
        _planRepoMock.Setup(r => r.GetByIdAsync(dto.InsurancePlanId)).ReturnsAsync(plan);
        _claimRepoMock.Setup(r => r.GetByCustomerIdAsync(1)).ReturnsAsync(new List<Claim>());

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _policyService.RequestPolicyAsync(1, dto));
    }
}
