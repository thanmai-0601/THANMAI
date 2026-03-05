using Application.DTOs.Policy;
using Xunit;
using System.Collections.Generic;

namespace Application.Tests.DTOs.Policy;

public class CreatePlanDtoTests
{
    [Fact]
    public void CreatePlanDto_PropertyAccessors_Work()
    {
        var dto = new CreatePlanDto 
        { 
            PlanName = "P", 
            Description = "D",
            MinSumAssured = 100000,
            MaxSumAssured = 1000000,
            TenureOptions = new List<int> { 10, 15 },
            MinEntryAge = 18,
            MaxEntryAge = 60,
            MinAnnualIncome = 50000,
            BaseRatePer1000 = 1.5m,
            LowRiskMultiplier = 1.0m,
            MediumRiskMultiplier = 1.2m,
            HighRiskMultiplier = 1.5m,
            CommissionPercentage = 5
        };
        Assert.Equal("P", dto.PlanName);
        Assert.Equal(100000, dto.MinSumAssured);
        Assert.NotEmpty(dto.TenureOptions);
    }
}
