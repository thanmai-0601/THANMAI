using Application.DTOs.Claim;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Services;
using Domain.Entities;
using Domain.Enums;
using Moq;
using Xunit;

namespace Application.Tests.Services;

public class ClaimServiceTests
{
    private readonly Mock<IClaimRepository> _claimRepoMock;
    private readonly Mock<IPolicyRepository> _policyRepoMock;
    private readonly Mock<IInvoiceRepository> _invoiceRepoMock;
    private readonly Mock<IClaimsOfficerAssignmentService> _assignmentMock;
    private readonly Mock<INotificationService> _notificationMock;
    private readonly Mock<IPremiumCalculationService> _premiumCalcMock;
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly ClaimService _claimService;

    public ClaimServiceTests()
    {
        _claimRepoMock = new Mock<IClaimRepository>();
        _policyRepoMock = new Mock<IPolicyRepository>();
        _invoiceRepoMock = new Mock<IInvoiceRepository>();
        _assignmentMock = new Mock<IClaimsOfficerAssignmentService>();
        _notificationMock = new Mock<INotificationService>();
        _premiumCalcMock = new Mock<IPremiumCalculationService>();
        _userRepoMock = new Mock<IUserRepository>();

        _claimService = new ClaimService(
            _claimRepoMock.Object,
            _policyRepoMock.Object,
            _invoiceRepoMock.Object,
            _assignmentMock.Object,
            _notificationMock.Object,
            _premiumCalcMock.Object,
            _userRepoMock.Object);
    }

    [Fact]
    public async Task RaiseClaimAsync_ShouldSucceed_WhenPolicyIsActiveAndNomineeMatches()
    {
        // Arrange
        var dto = new RaiseClaimDto 
        { 
            PolicyNumber = "POL-001", 
            CauseOfDeath = "Accident", 
            DateOfDeath = DateTime.UtcNow,
            NomineeName = "Jane Doe",
            NomineeRelationship = "Spouse",
            NomineeIdNumber = "123456789012",
            BankAccountName = "Jane Doe",
            BankAccountNumber = "12345",
            BankIfscCode = "IFSC001",
            NomineeIdProof = new ClaimDocumentDto { FileName = "proof.pdf", DocumentType = "ID", FileBase64 = "YmFzZTY0" },
            DeathCertificate = new ClaimDocumentDto { FileName = "death.pdf", DocumentType = "DeathInfo", FileBase64 = "YmFzZTY0" }
        };
        
        var policy = new Policy 
        { 
            Id = 10, 
            PolicyNumber = "POL-001", 
            CustomerId = 1, 
            Status = PolicyStatus.Active,
            Nominees = new List<Nominee> 
            { 
                new Nominee 
                { 
                    FullName = "Jane Doe", 
                    Relationship = "Spouse",
                    IdNumber = "123456789012",
                    Email = "jane.doe@example.com"
                } 
            }
        };
        
        _policyRepoMock.Setup(r => r.GetByPolicyNumberWithDetailsAsync(dto.PolicyNumber)).ReturnsAsync(policy);
        _claimRepoMock.Setup(r => r.GetByCustomerIdAsync(1)).ReturnsAsync(new List<Claim>());
        _assignmentMock.Setup(a => a.AssignOfficerAsync()).ReturnsAsync(20);
        _claimRepoMock.Setup(r => r.GenerateClaimNumberAsync()).ReturnsAsync("CLM-001");
        _claimRepoMock.Setup(r => r.GetByIdWithDetailsAsync(It.IsAny<int>())).ReturnsAsync(new Claim { Id = 1, ClaimNumber = "CLM-001" });

        // Act
        var result = await _claimService.RaiseClaimAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("CLM-001", result.ClaimNumber);
        _claimRepoMock.Verify(r => r.CreateAsync(It.IsAny<Claim>()), Times.Once);
    }

    [Fact]
    public async Task RaiseClaimAsync_ShouldThrow_WhenPolicyNotActive()
    {
        // Arrange
        var dto = new RaiseClaimDto { PolicyNumber = "POL-001" };
        var policy = new Policy { Id = 10, PolicyNumber = "POL-001", Status = PolicyStatus.Draft };
        _policyRepoMock.Setup(r => r.GetByPolicyNumberWithDetailsAsync(dto.PolicyNumber)).ReturnsAsync(policy);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _claimService.RaiseClaimAsync(dto));
    }
}
