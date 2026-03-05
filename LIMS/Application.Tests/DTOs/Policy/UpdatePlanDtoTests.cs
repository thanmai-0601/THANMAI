using Application.DTOs.Policy;
using Xunit;

namespace Application.Tests.DTOs.Policy;

public class UpdatePlanDtoTests
{
    [Fact]
    public void UpdatePlanDto_PropertyAccessors_Work()
    {
        var dto = new UpdatePlanDto 
        { 
            PlanName = "P", 
            IsActive = true,
            Description = "D",
            CommissionPercentage = 5,
            MinSumAssured = 100000,
            MaxSumAssured = 1000000
        };
        Assert.Equal("P", dto.PlanName);
        Assert.True(dto.IsActive);
    }
}
