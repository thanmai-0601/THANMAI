using Application.DTOs.Dashboard;
using Xunit;

namespace Application.Tests.DTOs.Dashboard;

public class AgentDashboardDtoTests
{
    [Fact]
    public void AgentDashboardDto_PropertyAccessors_Work()
    {
        var dto = new AgentDashboardDto { TotalAssignedPolicies = 5, TotalCommissionEarned = 100 };
        Assert.Equal(5, dto.TotalAssignedPolicies);
        Assert.Equal(100, dto.TotalCommissionEarned);
    }
}
