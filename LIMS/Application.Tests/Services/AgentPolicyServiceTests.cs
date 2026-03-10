using Application.DTOs.Policy;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Services;
using Domain.Entities;
using Domain.Enums;
using Moq;
using Xunit;

namespace Application.Tests.Services;

public class AgentPolicyServiceTests
{
    private readonly Mock<IPolicyRepository> _policyRepoMock;
    private readonly Mock<IPlanRepository> _planRepoMock;
    private readonly Mock<INomineeRepository> _nomineeRepoMock;
    private readonly Mock<ICommissionRepository> _commissionRepoMock;
    private readonly Mock<IPremiumCalculationService> _premiumCalcMock;
    private readonly Mock<IInvoiceService> _invoiceServiceMock;
    private readonly AgentPolicyService _service;

    public AgentPolicyServiceTests()
    {
        _policyRepoMock = new Mock<IPolicyRepository>();
        _planRepoMock = new Mock<IPlanRepository>();
        _nomineeRepoMock = new Mock<INomineeRepository>();
        _commissionRepoMock = new Mock<ICommissionRepository>();
        _premiumCalcMock = new Mock<IPremiumCalculationService>();
        _invoiceServiceMock = new Mock<IInvoiceService>();

        _service = new AgentPolicyService(
            _policyRepoMock.Object,
            _planRepoMock.Object,
            _nomineeRepoMock.Object,
            _commissionRepoMock.Object,
            _premiumCalcMock.Object,
            _invoiceServiceMock.Object);
    }

    [Fact]
    public async Task CalculatePremiumAsync_ShouldThrow_WhenAgentDoesNotOwnPolicy()
    {
        // Arrange
        var policy = new Policy { Id = 1, AgentId = 2 }; // Owned by Agent 2
        _policyRepoMock.Setup(r => r.GetByIdWithDetailsAsync(1)).ReturnsAsync(policy);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => 
            _service.CalculatePremiumAsync(1, 1, new AgentPremiumCalculationDto()));
    }

    [Fact]
    public async Task SubmitNomineesAsync_ShouldSucceed()
    {
        // Arrange
        var policy = new Policy { Id = 1, CustomerId = 1, Status = PolicyStatus.Submitted };
        _policyRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(policy);
        
        var dto = new SubmitNomineesDto
        {
            Nominee = new AddNomineeDto { FullName = "John Doe", Relationship = "Son", Age = 25, ContactNumber = "1234567890" }
        };

        // Act
        var result = await _service.SubmitNomineesAsync(1, 1, dto);

        // Assert
        Assert.Single(result);
        _nomineeRepoMock.Verify(r => r.DeleteByPolicyIdAsync(1), Times.Once);
        _nomineeRepoMock.Verify(r => r.AddRangeAsync(It.IsAny<List<Nominee>>()), Times.Once);
    }

    [Fact]
    public async Task MakeDecisionAsync_ShouldApprovePolicy_AndRecordCommission()
    {
        // Arrange
        var policy = new Policy 
        { 
            Id = 1, 
            AgentId = 1, 
            Status = PolicyStatus.Submitted, 
            InsurancePlan = new InsurancePlan { Id = 1 },
            PremiumAmount = 10000
        };
        _policyRepoMock.Setup(r => r.GetByIdWithDetailsAsync(1)).ReturnsAsync(policy);
        _premiumCalcMock.Setup(p => p.Calculate(It.IsAny<InsurancePlan>(), It.IsAny<decimal>(), It.IsAny<int>(), It.IsAny<string>()))
            .Returns(new PremiumCalculationResultDto { AnnualPremium = 10000 });
        
        var dto = new PolicyDecisionDto { IsApproved = true, RiskCategory = "Low" };

        // Act
        var result = await _service.MakeDecisionAsync(1, 1, dto);

        // Assert
        Assert.Equal("Approved", result.Status);
        _policyRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Policy>()), Times.Once);
        _commissionRepoMock.Verify(r => r.CreateAsync(It.IsAny<Commission>()), Times.Once);
        _invoiceServiceMock.Verify(i => i.GenerateScheduleAsync(1, PaymentFrequency.Annual), Times.Once);
    }
}
