using Application.DTOs.Endorsement;
using Application.Interfaces.Repositories;
using Application.Services;
using Domain.Entities;
using Domain.Enums;
using Moq;
using Xunit;

namespace Application.Tests.Services;

public class EndorsementServiceTests
{
    private readonly Mock<IEndorsementRepository> _endorsementRepoMock;
    private readonly Mock<IPolicyRepository> _policyRepoMock;
    private readonly Mock<INomineeRepository> _nomineeRepoMock;
    private readonly Mock<IClaimRepository> _claimRepoMock;
    private readonly EndorsementService _endorsementService;

    public EndorsementServiceTests()
    {
        _endorsementRepoMock = new Mock<IEndorsementRepository>();
        _policyRepoMock = new Mock<IPolicyRepository>();
        _nomineeRepoMock = new Mock<INomineeRepository>();
        _claimRepoMock = new Mock<IClaimRepository>();

        _endorsementService = new EndorsementService(
            _endorsementRepoMock.Object,
            _policyRepoMock.Object,
            _nomineeRepoMock.Object,
            _claimRepoMock.Object);
    }

    [Fact]
    public async Task RequestAddressChangeAsync_ShouldSucceed_WhenPolicyIsActive()
    {
        // Arrange
        var customerId = 1;
        var dto = new RequestAddressChangeDto { PolicyId = 10, NewAddress = "123 New St" };
        var policy = new Policy { Id = 10, CustomerId = customerId, Status = PolicyStatus.Active, CustomerAddress = "Old St" };
        
        _policyRepoMock.Setup(r => r.GetByIdWithDetailsAsync(dto.PolicyId)).ReturnsAsync(policy);
        _endorsementRepoMock.Setup(r => r.GetByIdWithDetailsAsync(It.IsAny<int>())).ReturnsAsync(new PolicyEndorsement { Id = 1 });

        // Act
        var result = await _endorsementService.RequestAddressChangeAsync(customerId, dto);

        // Assert
        Assert.NotNull(result);
        _endorsementRepoMock.Verify(r => r.CreateAsync(It.IsAny<PolicyEndorsement>()), Times.Once);
    }

    [Fact]
    public async Task RequestAddressChangeAsync_ShouldThrow_WhenNotPolicyOwner()
    {
        // Arrange
        var customerId = 1;
        var dto = new RequestAddressChangeDto { PolicyId = 10 };
        var policy = new Policy { Id = 10, CustomerId = 99 }; // Different owner
        _policyRepoMock.Setup(r => r.GetByIdWithDetailsAsync(dto.PolicyId)).ReturnsAsync(policy);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _endorsementService.RequestAddressChangeAsync(customerId, dto));
    }
}
