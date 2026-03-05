using Domain.Entities;
using Xunit;

namespace Domain.Tests.Entities;

public class CommissionTests
{
    [Fact]
    public void Commission_Properties_ShouldBeSetCorrectly()
    {
        // Arrange
        var commission = new Commission();
        var amount = 150m;
        var premium = 1000m;

        // Act
        commission.CommissionAmount = amount;
        commission.PremiumAmount = premium;

        // Assert
        Assert.Equal(amount, commission.CommissionAmount);
        Assert.Equal(premium, commission.PremiumAmount);
    }
}
