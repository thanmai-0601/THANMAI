using Application.Interfaces.Repositories;
using Application.Services;
using Domain.Entities;
using Moq;
using Xunit;

namespace Application.Tests.Services;

public class AgentAssignmentServiceTests
{
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IPolicyRepository> _policyRepoMock;
    private readonly AgentAssignmentService _service;

    public AgentAssignmentServiceTests()
    {
        _userRepoMock = new Mock<IUserRepository>();
        _policyRepoMock = new Mock<IPolicyRepository>();
        _service = new AgentAssignmentService(_userRepoMock.Object, _policyRepoMock.Object);
    }

    [Fact]
    public async Task AssignAgentAsync_ShouldThrow_WhenNoAgentsAvailable()
    {
        // Arrange
        _userRepoMock.Setup(r => r.GetActiveAgentsAsync()).ReturnsAsync(new List<User>());

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.AssignAgentAsync());
    }

    [Fact]
    public async Task AssignAgentAsync_ShouldReturnAgentWithLeastWorkload()
    {
        // Arrange
        var agents = new List<User>
        {
            new User { Id = 1 },
            new User { Id = 2 }
        };
        _userRepoMock.Setup(r => r.GetActiveAgentsAsync()).ReturnsAsync(agents);
        
        // Agent 1 has 10 policies, last assigned today
        _policyRepoMock.Setup(r => r.GetActiveCountByAgentAsync(1)).ReturnsAsync(10);
        _policyRepoMock.Setup(r => r.GetLastAssignmentDateAsync(1)).ReturnsAsync(DateTime.UtcNow);
        
        // Agent 2 has 5 policies, last assigned yesterday
        _policyRepoMock.Setup(r => r.GetActiveCountByAgentAsync(2)).ReturnsAsync(5);
        _policyRepoMock.Setup(r => r.GetLastAssignmentDateAsync(2)).ReturnsAsync(DateTime.UtcNow.AddDays(-1));

        // Act
        var result = await _service.AssignAgentAsync();

        // Assert
        Assert.Equal(2, result);
    }
}
