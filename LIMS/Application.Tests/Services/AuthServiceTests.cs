using Application.DTOs.Auth;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Services;
using Domain.Entities;
using Domain.Enums;
using Moq;
using Xunit;

namespace Application.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IJwtTokenService> _jwtServiceMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IPolicyRepository> _policyRepoMock;
    private readonly Mock<IClaimRepository> _claimRepoMock;
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _userRepoMock = new Mock<IUserRepository>();
        _jwtServiceMock = new Mock<IJwtTokenService>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _policyRepoMock = new Mock<IPolicyRepository>();
        _claimRepoMock = new Mock<IClaimRepository>();
        _notificationServiceMock = new Mock<INotificationService>();
        
        _authService = new AuthService(
            _userRepoMock.Object, 
            _jwtServiceMock.Object, 
            _passwordHasherMock.Object,
            _policyRepoMock.Object,
            _claimRepoMock.Object,
            _notificationServiceMock.Object
        );
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnAuthResponse_WhenCredentialsAreValid()
    {
        // Arrange
        var loginDto = new LoginDto { Email = "test@ex.com", Password = "password" };
        var user = new User { Id = 1, Email = "test@ex.com", FullName = "Test User", PasswordHash = "hashed", IsActive = true, Role = UserRole.Customer };
        
        _userRepoMock.Setup(r => r.GetByEmailAsync(loginDto.Email)).ReturnsAsync(user);
        _passwordHasherMock.Setup(p => p.Verify(loginDto.Password, user.PasswordHash)).Returns(true);
        _jwtServiceMock.Setup(j => j.GenerateToken(user)).Returns("mock-token");

        // Act
        var result = await _authService.LoginAsync(loginDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("mock-token", result.Token);
        Assert.Equal(user.Id, result.UserId);
        _userRepoMock.Verify(r => r.UpdateAsync(user), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_ShouldThrowUnauthorized_WhenUserNotFound()
    {
        // Arrange
        var loginDto = new LoginDto { Email = "nonexistent@ex.com", Password = "any" };
        _userRepoMock.Setup(r => r.GetByEmailAsync(loginDto.Email)).ReturnsAsync((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _authService.LoginAsync(loginDto));
    }

    [Fact]
    public async Task RegisterAsync_ShouldCreateUser_WhenEmailIsUnique()
    {
        // Arrange
        var registerDto = new RegisterDto { FullName = "New User", Email = "new@ex.com", Password = "pw", PhoneNumber = "123" };
        _userRepoMock.Setup(r => r.EmailExistsAsync(registerDto.Email)).ReturnsAsync(false);
        _passwordHasherMock.Setup(p => p.Hash(registerDto.Password)).Returns("hashed");

        // Act
        var result = await _authService.RegisterAsync(registerDto);

        // Assert
        Assert.NotNull(result);
        _userRepoMock.Verify(r => r.CreateAsync(It.Is<User>(u => u.Email == registerDto.Email.ToLower() && u.Role == UserRole.Customer)), Times.Once);
    }

    [Fact]
    public async Task ToggleUserStatusAsync_ShouldThrowInvalidOperationException_WhenAdminAttemptsDeactivation()
    {
        // Arrange
        var user = new User { Id = 1, Role = UserRole.Admin, IsActive = true };
        _userRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _authService.ToggleUserStatusAsync(1));
    }

    [Fact]
    public async Task ToggleUserStatusAsync_ShouldDeactivateAgentAndRejectTheirActivePolicies()
    {
        // Arrange
        var agent = new User { Id = 2, Role = UserRole.Agent, IsActive = true };
        var activePolicy = new Policy { Id = 101, Status = PolicyStatus.Active, AgentId = 2, CustomerId = 3, PolicyNumber = "POL-01" };
        var draftPolicy = new Policy { Id = 102, Status = PolicyStatus.Draft, AgentId = 2, CustomerId = 4, PolicyNumber = "POL-02" };
        
        _userRepoMock.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(agent);
        _policyRepoMock.Setup(r => r.GetByAgentIdAsync(2)).ReturnsAsync(new List<Policy> { activePolicy, draftPolicy });

        // Act
        await _authService.ToggleUserStatusAsync(2);

        // Assert
        Assert.False(agent.IsActive);
        _userRepoMock.Verify(r => r.UpdateAsync(agent), Times.Once);
        
        Assert.Equal(PolicyStatus.Rejected, activePolicy.Status);
        _policyRepoMock.Verify(r => r.UpdateRangeAsync(It.Is<IEnumerable<Policy>>(l => l.Contains(activePolicy))), Times.Once);
        _notificationServiceMock.Verify(n => n.CreateNotificationAsync(3, It.IsAny<string>()), Times.Once);

        Assert.Equal(PolicyStatus.Draft, draftPolicy.Status); // Should not change
        _policyRepoMock.Verify(r => r.UpdateAsync(draftPolicy), Times.Never);
    }

    [Fact]
    public async Task ToggleUserStatusAsync_ShouldDeactivateClaimsOfficerAndRejectTheirActiveClaims()
    {
        // Arrange
        var officer = new User { Id = 2, Role = UserRole.ClaimsOfficer, IsActive = true };
        var activeClaim = new Claim { Id = 101, Status = ClaimStatus.UnderReview, ClaimsOfficerId = 2, CustomerId = 3, ClaimNumber = "CLM-01" };
        var settledClaim = new Claim { Id = 102, Status = ClaimStatus.Settled, ClaimsOfficerId = 2, CustomerId = 4, ClaimNumber = "CLM-02" };
        
        _userRepoMock.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(officer);
        _claimRepoMock.Setup(r => r.GetByOfficerIdAsync(2)).ReturnsAsync(new List<Claim> { activeClaim, settledClaim });

        // Act
        await _authService.ToggleUserStatusAsync(2);

        // Assert
        Assert.False(officer.IsActive);
        _userRepoMock.Verify(r => r.UpdateAsync(officer), Times.Once);
        
        Assert.Equal(ClaimStatus.Rejected, activeClaim.Status);
        _claimRepoMock.Verify(r => r.UpdateRangeAsync(It.Is<IEnumerable<Claim>>(l => l.Contains(activeClaim))), Times.Once);
        _notificationServiceMock.Verify(n => n.CreateNotificationAsync(3, It.IsAny<string>()), Times.Once);

        Assert.Equal(ClaimStatus.Settled, settledClaim.Status); // Should not change
        _claimRepoMock.Verify(r => r.UpdateAsync(settledClaim), Times.Never);
    }
    [Fact]
    public async Task ToggleUserStatusAsync_ShouldRestoreAgentPolicies_WhenReactivated()
    {
        // Arrange
        var agent = new User { Id = 2, Role = UserRole.Agent, IsActive = false };
        var recoveredPolicy = new Policy 
        { 
            Id = 101, 
            Status = PolicyStatus.Rejected, 
            RejectionReason = "RECOVERY_AGENT_OFFLINE|Active", 
            AgentId = 2, 
            CustomerId = 3, 
            PolicyNumber = "POL-01" 
        };
        
        _userRepoMock.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(agent);
        _policyRepoMock.Setup(r => r.GetByAgentIdAsync(2)).ReturnsAsync(new List<Policy> { recoveredPolicy });

        // Act
        await _authService.ToggleUserStatusAsync(2);

        // Assert
        Assert.True(agent.IsActive);
        Assert.Equal(PolicyStatus.Active, recoveredPolicy.Status);
        Assert.Null(recoveredPolicy.RejectionReason);
        _policyRepoMock.Verify(r => r.UpdateRangeAsync(It.Is<IEnumerable<Policy>>(l => l.Contains(recoveredPolicy))), Times.Once);
        _notificationServiceMock.Verify(n => n.CreateNotificationAsync(3, It.IsAny<string>()), Times.Once);
    }
}
