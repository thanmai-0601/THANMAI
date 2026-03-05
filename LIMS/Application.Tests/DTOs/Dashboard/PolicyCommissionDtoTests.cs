using Application.DTOs.Dashboard;
using Xunit;

namespace Application.Tests.DTOs.Dashboard;

public class PolicyCommissionDtoTests
{
    [Fact]
    public void PolicyCommissionDto_PropertyAccessors_Work()
    {
        var dto = new PolicyCommissionDto { PolicyNumber = "P", CommissionAmount = 50.5m };
        Assert.Equal("P", dto.PolicyNumber);
        Assert.Equal(50.5m, dto.CommissionAmount);
    }
}
