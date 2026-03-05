using Domain.Entities;
using Xunit;

namespace Domain.Tests.Entities;

public class InsurancePlanTests
{
    [Fact]
    public void InsurancePlan_Initialization_ShouldBeActiveByDefault()
    {
        // Arrange & Act
        var plan = new InsurancePlan();

        // Assert
        Assert.True(plan.IsActive);
    }

    [Fact]
    public void InsurancePlan_Properties_ShouldBeSetCorrectly()
    {
        // Arrange
        var plan = new InsurancePlan();
        var name = "Term Life Plus";
        var minAge = 18;

        // Act
        plan.PlanName = name;
        plan.MinEntryAge = minAge;

        // Assert
        Assert.Equal(name, plan.PlanName);
        Assert.Equal(minAge, plan.MinEntryAge);
    }
}
