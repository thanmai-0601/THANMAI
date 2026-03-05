using Application.DTOs.Dashboard;
using Xunit;

namespace Application.Tests.DTOs.Dashboard;

public class AdminDashboardDtoTests
{
    [Fact]
    public void AdminDashboardDto_PropertyAccessors_Work()
    {
        var dto = new AdminDashboardDto { TotalPolicies = 10, TotalPremiumCollected = 1000 };
        Assert.Equal(10, dto.TotalPolicies);
        Assert.Equal(1000, dto.TotalPremiumCollected);
    }
}
