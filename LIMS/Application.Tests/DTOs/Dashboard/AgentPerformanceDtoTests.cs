using Application.DTOs.Dashboard;
using Xunit;

namespace Application.Tests.DTOs.Dashboard;

public class AgentPerformanceDtoTests
{
    [Fact]
    public void AgentPerformanceDto_PropertyAccessors_Work()
    {
        var dto = new AgentPerformanceDto { AgentId = 1, TotalCommissionEarned = 100 };
        Assert.Equal(1, dto.AgentId);
        Assert.Equal(100, dto.TotalCommissionEarned);
    }
}
