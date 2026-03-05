using Domain.Entities;
using Domain.Enums;
using Xunit;

namespace Domain.Tests.Entities;

public class PolicyTests
{
    [Fact]
    public void Policy_Initialization_ShouldHaveDefaultStatus()
    {
        // Arrange & Act
        var policy = new Policy();

        // Assert
        Assert.Equal(PolicyStatus.Draft, policy.Status);
        Assert.NotNull(policy.Nominees);
        Assert.NotNull(policy.Documents);
    }

    [Fact]
    public void Policy_Properties_ShouldBeSetCorrectly()
    {
        // Arrange
        var policy = new Policy();
        var policyNumber = "POL-20240001";
        var sumAssured = 1000000m;

        // Act
        policy.PolicyNumber = policyNumber;
        policy.SumAssured = sumAssured;

        // Assert
        Assert.Equal(policyNumber, policy.PolicyNumber);
        Assert.Equal(sumAssured, policy.SumAssured);
    }
}
