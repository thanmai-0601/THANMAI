using Application.Interfaces.Repositories;
using Application.Services;
using Domain.Entities;
using Moq;
using Xunit;

namespace Application.Tests.Services;

public class ClaimsOfficerAssignmentServiceTests
{
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IClaimRepository> _claimRepoMock;
    private readonly ClaimsOfficerAssignmentService _service;

    public ClaimsOfficerAssignmentServiceTests()
    {
        _userRepoMock = new Mock<IUserRepository>();
        _claimRepoMock = new Mock<IClaimRepository>();
        _service = new ClaimsOfficerAssignmentService(_userRepoMock.Object, _claimRepoMock.Object);
    }

    [Fact]
    public async Task AssignOfficerAsync_ShouldThrow_WhenNoOfficersAvailable()
    {
        // Arrange
        _userRepoMock.Setup(r => r.GetActiveClaimsOfficersAsync()).ReturnsAsync(new List<User>());

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.AssignOfficerAsync());
    }

    [Fact]
    public async Task AssignOfficerAsync_ShouldReturnOfficerWithLeastClaims()
    {
        // Arrange
        var officers = new List<User> { new User { Id = 10 }, new User { Id = 11 } };
        _userRepoMock.Setup(r => r.GetActiveClaimsOfficersAsync()).ReturnsAsync(officers);
        
        _claimRepoMock.Setup(r => r.GetActiveCountByOfficerAsync(10)).ReturnsAsync(5);
        _claimRepoMock.Setup(r => r.GetActiveCountByOfficerAsync(11)).ReturnsAsync(2);

        // Act
        var result = await _service.AssignOfficerAsync();

        // Assert
        Assert.Equal(11, result);
    }
}
