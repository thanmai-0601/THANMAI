using Application.DTOs.Policy;
using Application.Interfaces.Repositories;
using Application.Services;
using Domain.Entities;
using Moq;
using Xunit;

namespace Application.Tests.Services;

public class PlanServiceTests
{
    private readonly Mock<IPlanRepository> _planRepoMock;
    private readonly PlanService _planService;

    public PlanServiceTests()
    {
        _planRepoMock = new Mock<IPlanRepository>();
        _planService = new PlanService(_planRepoMock.Object);
    }

    [Fact]
    public async Task GetPlanByIdAsync_ShouldReturnPlan_WhenExists()
    {
        // Arrange
        var planId = 1;
        var plan = new InsurancePlan { Id = planId, PlanName = "Life Plan", TenureOptions = "10,20" };
        _planRepoMock.Setup(r => r.GetByIdAsync(planId)).ReturnsAsync(plan);

        // Act
        var result = await _planService.GetPlanByIdAsync(planId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Life Plan", result.PlanName);
    }

    [Fact]
    public async Task CreatePlanAsync_ShouldThrow_WhenSumAssuredRangeInvalid()
    {
        // Arrange
        var dto = new CreatePlanDto { MinSumAssured = 1000, MaxSumAssured = 500 }; // Invalid

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _planService.CreatePlanAsync(dto));
    }
}
